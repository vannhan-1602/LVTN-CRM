using MediatR;

namespace CRM.Application.Features.Tickets.Commands.DeleteTicket
{
    public record DeleteTicketCommand(ulong Id) : IRequest<bool>;
}