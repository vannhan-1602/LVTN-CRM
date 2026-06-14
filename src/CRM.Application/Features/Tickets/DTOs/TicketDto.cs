using System;
using System.Collections.Generic;

namespace CRM.Application.Features.Tickets.DTOs
{
    public class TicketDto
    {
        public ulong Id { get; set; }
        public string MaTicket { get; set; } = string.Empty;
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? FileDinhKem { get; set; }
        public ushort? LoaiTicketId { get; set; }
        public ulong KhachHangId { get; set; }
        public ulong? HopDongId { get; set; }
        public uint? SanPhamId { get; set; }
        public string MucDoUuTien { get; set; } = string.Empty;
        public string NguonTiepNhan { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public uint? NhanVienTiepNhanId { get; set; }
        public uint? NhanVienXuLyId { get; set; }
        public DateTime? NgayHenXuLy { get; set; }
        public DateTime? NgayDong { get; set; }
        public string? LyDoDong { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TicketPhanHoiDto
    {
        public ulong Id { get; set; }
        public ulong TicketId { get; set; }
        public uint? NguoiPhanHoiId { get; set; }
        public string LoaiPhanHoi { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string? FileDinhKem { get; set; }
        public string? TrangThaiTruoc { get; set; }
        public string? TrangThaiSau { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateTicketRequestDto
    {
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? FileDinhKem { get; set; }
        public ushort? LoaiTicketId { get; set; }
        public ulong KhachHangId { get; set; }
        public ulong? HopDongId { get; set; }
        public uint? SanPhamId { get; set; }
        public string? MucDoUuTien { get; set; }
        public string? NguonTiepNhan { get; set; }
        public uint? NhanVienTiepNhanId { get; set; }
        public uint? NhanVienXuLyId { get; set; }
        public DateTime? NgayHenXuLy { get; set; }
    }

    public class UpdateTicketRequestDto
    {
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? FileDinhKem { get; set; }
        public ushort? LoaiTicketId { get; set; }
        public ulong? HopDongId { get; set; }
        public uint? SanPhamId { get; set; }
        public string? MucDoUuTien { get; set; }
        public string? NguonTiepNhan { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayHenXuLy { get; set; }
    }

    public class AssignTicketRequestDto
    {
        public uint NhanVienXuLyId { get; set; }
        public DateTime? NgayHenXuLy { get; set; }
    }

    public class AddPhanHoiRequestDto
    {
        public string LoaiPhanHoi { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string? FileDinhKem { get; set; }
    }

    public class CloseTicketRequestDto
    {
        public string? LyDoDong { get; set; }
    }
}