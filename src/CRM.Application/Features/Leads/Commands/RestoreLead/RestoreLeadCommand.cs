using MediatR;

namespace CRM.Application.Features.Leads.Commands.RestoreLead
{
    public record RestoreLeadCommand(ulong Id) : IRequest<bool>;
}
