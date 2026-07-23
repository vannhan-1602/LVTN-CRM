using CRM.Application.Features.Invoices.DTOs;
using CRM.Domain.Entities.Sales;

namespace CRM.Application.Features.Invoices.Mappings;

public static class InvoiceMapper
{
    public static InvoiceDto ToDto(
        HoaDon h, string? tenKhachHang = null, string? maHopDong = null, int? soDot = null) => new()
    {
        Id = h.Id,
        MaHoaDon = h.MaHoaDon,
        HopDongId = h.HopDongId,
        MaHopDong = maHopDong,
        LichThanhToanId = h.LichThanhToanId,
        SoDot = soDot,
        KhachHangId = h.KhachHangId,
        TenKhachHang = tenKhachHang,
        TongTien = h.TongTien,
        SoTienDaThu = h.SoTienDaThu,
        TrangThaiThanhToan = h.TrangThaiThanhToan,
        CreatedAt = h.CreatedAt,
        UpdatedAt = h.UpdatedAt
    };
}
