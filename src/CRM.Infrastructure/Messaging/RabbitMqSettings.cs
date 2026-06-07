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
}
