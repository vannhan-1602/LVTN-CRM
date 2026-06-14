using CRM.Application.Features.Tickets.DTOs;
using MediatR;

namespace CRM.Application.Features.Tickets.Commands.CloseTicket
{
    public record CloseTicketCommand(
        ulong Id,
        uint? NguoiDongId,
        string? LyDoDong) : IRequest<TicketDto>;
}