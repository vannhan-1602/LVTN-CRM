using CRM.Application.Features.Quotes.DTOs;
using CRM.Domain.Entities.Sales;

namespace CRM.Application.Features.Quotes.Mappings;

public static class QuoteMapper
{
    public static QuoteDto ToDto(BaoGia q, string? tenKhachHang = null, string? tenNhanVien = null) => new()
    {
        Id = q.Id,
        MaBaoGia = q.MaBaoGia,
        KhachHangId = q.KhachHangId,
        TenKhachHang = tenKhachHang,
        TongTien = q.TongTien,
        TrangThai = q.TrangThai,
        NhanVienId = q.NhanVienId,
        TenNhanVien = tenNhanVien,
        CreatedAt = q.CreatedAt,
        UpdatedAt = q.UpdatedAt
    };
}
