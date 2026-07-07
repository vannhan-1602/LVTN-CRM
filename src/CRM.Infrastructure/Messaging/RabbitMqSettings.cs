namespace CRM.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string AuditLogQueue { get; set; } = "crm.audit-log";
    public string NotificationQueue { get; set; } = "crm.notifications";

    // ── Dead-letter (DLQ) ────────────────────────────────────────────────────
    // Message xử lý thất bại (lỗi deserialize, lỗi DB không phục hồi được...) sẽ
    // được route sang exchange/queue này thay vì bị requeue vô hạn vào lại
    // AuditLogQueue (gây vòng lặp retry vô tận, tốn CPU, không ai biết message
    // đang lỗi). Message trong DLQ có thể xem/replay thủ công sau.
    public string DeadLetterExchange { get; set; } = "crm.dlx";
    public string AuditLogDeadLetterQueue { get; set; } = "crm.audit-log.dlq";
}
