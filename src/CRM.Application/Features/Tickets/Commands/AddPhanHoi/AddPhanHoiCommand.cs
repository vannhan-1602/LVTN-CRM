using CRM.Application.Features.Tickets.DTOs;
using MediatR;

namespace CRM.Application.Features.Tickets.Commands.AddPhanHoi
{
    public record AddPhanHoiCommand(
        ulong TicketId,
        uint? NguoiPhanHoiId,
        string LoaiPhanHoi,
        string NoiDung,
        string? FileDinhKem) : IRequest<TicketPhanHoiDto>;
}