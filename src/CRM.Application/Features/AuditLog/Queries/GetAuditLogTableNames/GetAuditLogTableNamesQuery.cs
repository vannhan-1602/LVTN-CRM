using MediatR;

namespace CRM.Application.Features.AuditLog.Queries.GetAuditLogTableNames;

public record GetAuditLogTableNamesQuery : IRequest<List<string>>;
