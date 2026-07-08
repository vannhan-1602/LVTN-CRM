using CRM.Application.Common.Models;
using CRM.Application.Features.AuditLog.DTOs;
using MediatR;

namespace CRM.Application.Features.AuditLog.Queries.GetAuditLogs;

public record GetAuditLogsQuery(
    int PageNumber,
    int PageSize,
    string? TableName,
    string? Action,
    uint? UserId,
    ulong? RecordId,
    DateTime? TuNgay,
    DateTime? DenNgay
) : IRequest<PagedResult<AuditLogDto>>;
