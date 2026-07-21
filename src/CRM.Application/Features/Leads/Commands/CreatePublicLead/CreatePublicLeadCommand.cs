using MediatR;

namespace CRM.Application.Features.Leads.Commands.CreatePublicLead;

public record CreatePublicLeadCommand(
    string TenLead,
    string? TenCongTy,
    string? SoDienThoai,
    string? Email
) : IRequest<ulong>;