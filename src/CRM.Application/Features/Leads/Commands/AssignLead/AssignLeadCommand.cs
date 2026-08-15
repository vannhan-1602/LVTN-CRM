using CRM.Application.Features.Leads.DTOs;
using MediatR;

namespace CRM.Application.Features.Leads.Commands.AssignLead;

public record AssignLeadCommand(
    ulong Id,
    uint? NhanVienPhuTrachId) : IRequest<LeadDto>;