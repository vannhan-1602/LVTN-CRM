using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CRM.Infrastructure.Messaging;

/// <summary>
/// Khai báo topology (exchange/queue) dùng chung giữa Publisher và Consumer.
/// QUAN TRỌNG: RabbitMQ yêu cầu arguments của 1 queue phải GIỐNG HỆT nhau giữa mọi
/// lần declare (publisher lẫn consumer) — khai báo lệch nhau sẽ ném
/// PRECONDITION_FAILED và làm sập kết nối. Vì vậy cả 2 phía đều phải gọi đúng qua
/// helper này thay vì tự gọi QueueDeclareAsync riêng lẻ.
/// </summary>
public static class RabbitMqTopology
{
    /// <summary>
    /// Đảm bảo tồn tại: DLX (direct exchange) + DLQ (bind vào DLX) + AuditLogQueue
    /// (có arg x-dead-letter-exchange trỏ về DLX). Message bị nack(requeue:false) từ
    /// AuditLogQueue sẽ tự động rơi vào DLQ thay vì bị RabbitMQ hủy âm thầm.
    /// Idempotent — gọi lại nhiều lần với cùng tham số không lỗi.
    /// </summary>
    public static async Task EnsureAuditLogTopologyAsync(
        IChannel channel, RabbitMqSettings settings, CancellationToken ct = default)
    {
        await channel.ExchangeDeclareAsync(
            exchange: settings.DeadLetterExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueDeclareAsync(
            queue: settings.AuditLogDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: settings.AuditLogDeadLetterQueue,
            exchange: settings.DeadLetterExchange,
            routingKey: settings.AuditLogQueue,
            cancellationToken: ct);

        await channel.QueueDeclareAsync(
            queue: settings.AuditLogQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = settings.DeadLetterExchange,
                ["x-dead-letter-routing-key"] = settings.AuditLogQueue,
            },
            cancellationToken: ct);
    }
}
