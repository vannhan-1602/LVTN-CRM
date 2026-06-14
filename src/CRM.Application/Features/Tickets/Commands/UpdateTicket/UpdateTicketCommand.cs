using CRM.Application.Features.Tickets.DTOs;
using MediatR;

namespace CRM.Application.Features.Tickets.Commands.UpdateTicket
{
    public record UpdateTicketCommand(
        ulong Id,
        string TieuDe,
        string? MoTa,
        string? FileDinhKem,
        ushort? LoaiTicketId,
        ulong? HopDongId,
        uint? SanPhamId,
        string? MucDoUuTien,
        string? NguonTiepNhan,
        string? TrangThai,
        DateTime? NgayHenXuLy) : IRequest<TicketDto>;
}