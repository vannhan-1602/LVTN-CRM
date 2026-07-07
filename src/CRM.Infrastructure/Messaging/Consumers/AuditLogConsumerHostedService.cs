using System.Text;
using System.Text.Json;
using CRM.Infrastructure.Messaging;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CRM.Infrastructure.Messaging.Consumers;

public class AuditLogConsumerHostedService : BackgroundService
{
    private const string RetryCountHeader = "x-retry-count";
    private const int MaxRetries = 3;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<AuditLogConsumerHostedService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public AuditLogConsumerHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> settings,
        ILogger<AuditLogConsumerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit log consumer disconnected. Retrying in 5 seconds...");
                await CleanupAsync();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConnectAndConsumeAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Declare qua topology dùng chung (khai báo cả DLX + DLQ + queue chính với
        // arg x-dead-letter-exchange) — bắt buộc để arguments khớp với Publisher.
        await RabbitMqTopology.EnsureAuditLogTopologyAsync(_channel, _settings, stoppingToken);

        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<AuditLogMessage>(json);

                if (message is not null)
                {
                    await PersistAuditLogAsync(message, stoppingToken);
                }

                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                await HandleFailureAsync(eventArgs, ex, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _settings.AuditLogQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Audit log consumer started on queue {QueueName}", _settings.AuditLogQueue);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>
    /// Trước đây: nack(requeue:true) vô điều kiện → message lỗi vĩnh viễn (vd JSON hỏng)
    /// bị requeue lặp lại VÔ HẠN, tốn CPU và không ai biết nó đang lỗi.
    /// Giờ: đếm số lần retry qua header, retry tối đa <see cref="MaxRetries"/> lần rồi
    /// đẩy sang DLQ (crm.audit-log.dlq) để xem/xử lý thủ công thay vì lặp vô hạn.
    /// </summary>
    private async Task HandleFailureAsync(BasicDeliverEventArgs eventArgs, Exception ex, CancellationToken ct)
    {
        var headers = eventArgs.BasicProperties.Headers;
        var retryCount = 0;
        if (headers is not null && headers.TryGetValue(RetryCountHeader, out var raw))
        {
            retryCount = raw switch
            {
                int i => i,
                long l => (int)l,
                byte[] b => int.Parse(Encoding.UTF8.GetString(b)),
                _ => 0
            };
        }

        if (retryCount >= MaxRetries)
        {
            _logger.LogError(ex,
                "Audit log message failed after {RetryCount} retries, sending to DLQ {DlqName}",
                retryCount, _settings.AuditLogDeadLetterQueue);
            // requeue:false + queue có x-dead-letter-exchange => RabbitMQ tự route sang DLQ
            await _channel!.BasicNackAsync(eventArgs.DeliveryTag, false, false, ct);
            return;
        }

        _logger.LogWarning(ex,
            "Failed to process audit log message (attempt {Attempt}/{Max}), retrying",
            retryCount + 1, MaxRetries);

        // Không thể "sửa header rồi requeue" bằng BasicNack (header giữ nguyên bản gốc),
        // nên ack bản cũ rồi tự publish lại với header đã tăng retry count.
        // Dựng BasicProperties mới thủ công (không dùng copy-constructor từ
        // IReadOnlyBasicProperties) để chắc chắn tương thích với RabbitMQ.Client 7.x.
        var newHeaders = new Dictionary<string, object?>(headers ?? new Dictionary<string, object?>())
        {
            [RetryCountHeader] = retryCount + 1
        };
        var newProps = new BasicProperties
        {
            Persistent = true,
            ContentType = eventArgs.BasicProperties.ContentType,
            Headers = newHeaders
        };

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _settings.AuditLogQueue,
            mandatory: false,
            basicProperties: newProps,
            body: eventArgs.Body,
            cancellationToken: ct);

        await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, ct);
    }

    private async Task PersistAuditLogAsync(AuditLogMessage message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();

        var auditLog = new SysAuditLogEntity
        {
            TableName = message.TableName,
            RecordId = message.RecordId,
            Action = message.Action,
            OldData = message.OldData,
            NewData = message.NewData,
            UserId = message.UserId,
            ChangedAt = message.ChangedAt
        };

        context.SysAuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Persisted audit log {Action} on {TableName}#{RecordId}",
            message.Action,
            message.TableName,
            message.RecordId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await CleanupAsync();
        await base.StopAsync(cancellationToken);
    }

    private async Task CleanupAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
            _channel = null;
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
            _connection = null;
        }
    }
}