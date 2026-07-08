using CRM.Application.Common.Models;
using CRM.Application.Features.AuditLog.DTOs;
using CRM.Application.Interfaces.Audit;
using MediatR;

namespace CRM.Application.Features.AuditLog.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IAuditLogReader _reader;
    public GetAuditLogsQueryHandler(IAuditLogReader reader) => _reader = reader;

    public Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken ct) =>
        _reader.GetPagedAsync(
            request.PageNumber, request.PageSize, request.TableName, request.Action,
            request.UserId, request.RecordId, request.TuNgay, request.DenNgay, ct);
}
