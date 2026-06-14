using CRM.Application.Features.Tickets.DTOs;
using MediatR;

namespace CRM.Application.Features.Tickets.Commands.AssignTicket
{
    public record AssignTicketCommand(
        ulong Id,
        uint NhanVienXuLyId,
        DateTime? NgayHenXuLy) : IRequest<TicketDto>;
}