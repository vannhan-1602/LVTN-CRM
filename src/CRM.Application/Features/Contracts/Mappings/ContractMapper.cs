using CRM.Application.Features.Contracts.DTOs;
using CRM.Domain.Entities.Sales;

namespace CRM.Application.Features.Contracts.Mappings;

public static class ContractMapper
{
    public static ContractDto ToDto(
        HopDong c, string? tenKhachHang = null, string? maBaoGia = null, decimal? giaTri = null) => new()
    {
        Id = c.Id,
        MaHopDong = c.MaHopDong,
        KhachHangId = c.KhachHangId,
        TenKhachHang = tenKhachHang,
        BaoGiaId = c.BaoGiaGocId,
        MaBaoGia = maBaoGia,
        GiaTri = giaTri,
        NgayKy = c.NgayKy,
        ThoiHan = c.ThoiHan,
        TrangThai = c.TrangThai,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
