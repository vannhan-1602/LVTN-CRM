using System.Text;
using System.Text.Json;
using CRM.Domain.Interfaces.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CRM.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqPublisher(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
    }

    public async Task PublishAsync<TMessage>(
        string queueName,
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : class
    {
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = new BasicProperties { Persistent = true };

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Published message to queue {QueueName}", queueName);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
