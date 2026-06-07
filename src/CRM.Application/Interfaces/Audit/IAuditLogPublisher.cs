namespace CRM.Application.Interfaces.Audit;

public interface IAuditLogPublisher
{
    Task PublishAsync(
        string tableName,
        ulong recordId,
        string action,
        object? oldData,
        object? newData,
        CancellationToken cancellationToken = default);
}
