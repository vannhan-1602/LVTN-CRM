using CRM.Application.Features.Products.DTOs;
using CRM.Domain.Entities.Products;

namespace CRM.Application.Features.Products.Mappings;

public static class ProductMapper
{
    public static ProductDto ToDto(SanPham p, string? tenLoai = null, string? anhDaiDien = null) => new()
    {
        Id = p.Id,
        LoaiSanPhamId = p.LoaiSanPhamId,
        TenLoai = tenLoai,
        MaSP = p.MaSP,
        TenSP = p.TenSP,
        DonVi = p.DonVi,
        GiaBan = p.GiaBan,
        SoLuongTon = p.SoLuongTon,
        DangKinhDoanh = p.DangKinhDoanh,
        AnhDaiDien = anhDaiDien,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    public static ProductTypeDto ToDto(LoaiSanPham l) => new()
    {
        Id = l.Id,
        TenLoai = l.TenLoai,
        MoTa = l.MoTa
    };
}
