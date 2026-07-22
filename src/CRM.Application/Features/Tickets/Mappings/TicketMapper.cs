using CRM.Application.Features.Tickets.DTOs;
using CRM.Domain.Entities.Tickets;

namespace CRM.Application.Features.Tickets.Mappings
{
    public static class TicketMapper
    {
        public static TicketDto ToDto(Ticket ticket) => new()
        {
            Id = ticket.Id,
            MaTicket = ticket.MaTicket,
            TieuDe = ticket.TieuDe,
            MoTa = ticket.MoTa,
            FileDinhKem = ticket.FileDinhKem,
            LoaiTicketId = ticket.LoaiTicketId,
            KhachHangId = ticket.KhachHangId,
            HopDongId = ticket.HopDongId,
            SanPhamId = ticket.SanPhamId,
            MucDoUuTien = ticket.MucDoUuTien.ToString(),
            NguonTiepNhan = ticket.NguonTiepNhan.ToString(),
            TrangThai = ticket.TrangThai.ToString(),
            NhanVienTiepNhanId = ticket.NhanVienTiepNhanId,
            NhanVienXuLyId = ticket.NhanVienXuLyId,
            NgayHenXuLy = ticket.NgayHenXuLy,
            ThoiHanSLA = ticket.ThoiHanSLA,
            SoLanEscalate = ticket.SoLanEscalate,
            NgayDong = ticket.NgayDong,
            LyDoDong = ticket.LyDoDong,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };

        public static TicketPhanHoiDto ToDto(TicketPhanHoi phanHoi) => new()
        {
            Id = phanHoi.Id,
            TicketId = phanHoi.TicketId,
            NguoiPhanHoiId = phanHoi.NguoiPhanHoiId,
            LoaiPhanHoi = phanHoi.LoaiPhanHoi.ToString(),
            NoiDung = phanHoi.NoiDung,
            FileDinhKem = phanHoi.FileDinhKem,
            TrangThaiTruoc = phanHoi.TrangThaiTruoc?.ToString(),
            TrangThaiSau = phanHoi.TrangThaiSau?.ToString(),
            CreatedAt = phanHoi.CreatedAt
        };
    }
}