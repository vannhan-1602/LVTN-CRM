using System.Text.Json;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Domain.Interfaces.Messaging;
using CRM.Infrastructure.Messaging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Messaging;

public class AuditLogPublisher : IAuditLogPublisher
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly ICurrentUserService _currentUserService;
    private readonly RabbitMqSettings _settings;

    public AuditLogPublisher(
        IMessagePublisher messagePublisher,
        ICurrentUserService currentUserService,
        IOptions<RabbitMqSettings> settings)
    {
        _messagePublisher = messagePublisher;
        _currentUserService = currentUserService;
        _settings = settings.Value;
    }

    public Task PublishAsync(
        string tableName,
        ulong recordId,
        string action,
        object? oldData,
        object? newData,
        CancellationToken cancellationToken = default)
    {
        var message = new AuditLogMessage
        {
            TableName = tableName,
            RecordId = recordId,
            Action = action,
            OldData = oldData is null ? null : JsonSerializer.Serialize(oldData),
            NewData = newData is null ? null : JsonSerializer.Serialize(newData),
            UserId = _currentUserService.UserId,
            ChangedAt = DateTime.UtcNow
        };

        return _messagePublisher.PublishAsync(_settings.AuditLogQueue, message, cancellationToken);
    }
}
