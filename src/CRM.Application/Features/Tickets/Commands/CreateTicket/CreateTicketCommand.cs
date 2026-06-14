using CRM.Application.Features.Tickets.DTOs;
using MediatR;

namespace CRM.Application.Features.Tickets.Commands.CreateTicket
{
    public record CreateTicketCommand(
        string TieuDe,
        string? MoTa,
        string? FileDinhKem,
        ushort? LoaiTicketId,
        ulong KhachHangId,
        ulong? HopDongId,
        uint? SanPhamId,
        string? MucDoUuTien,
        string? NguonTiepNhan,
        uint? NhanVienTiepNhanId,
        uint? NhanVienXuLyId,
        DateTime? NgayHenXuLy) : IRequest<TicketDto>;
}