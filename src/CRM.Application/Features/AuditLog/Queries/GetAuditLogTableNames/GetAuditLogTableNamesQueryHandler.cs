using CRM.Application.Interfaces.Audit;
using MediatR;

namespace CRM.Application.Features.AuditLog.Queries.GetAuditLogTableNames;

public class GetAuditLogTableNamesQueryHandler : IRequestHandler<GetAuditLogTableNamesQuery, List<string>>
{
    private readonly IAuditLogReader _reader;
    public GetAuditLogTableNamesQueryHandler(IAuditLogReader reader) => _reader = reader;

    public Task<List<string>> Handle(GetAuditLogTableNamesQuery request, CancellationToken ct) =>
        _reader.GetDistinctTableNamesAsync(ct);
}
