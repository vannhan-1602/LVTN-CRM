using CRM.Application.Features.PhieuThuChi.DTOs;
using DomainPhieuThuChi = CRM.Domain.Entities.Sales.PhieuThuChi;

namespace CRM.Application.Features.PhieuThuChi.Mappings;

public static class PhieuThuChiMapper
{
    public static PhieuThuChiDto ToDto(
        DomainPhieuThuChi p,
        string? tenKhachHang = null,
        string? maHoaDon = null,
        string? tenNguoiLap = null) => new()
    {
        Id = p.Id,
        MaPhieu = p.MaPhieu,
        LoaiPhieu = p.LoaiPhieu,
        KhachHangId = p.KhachHangId,
        TenKhachHang = tenKhachHang,
        HoaDonId = p.HoaDonId,
        MaHoaDon = maHoaDon,
        SoTien = p.SoTien,
        NguoiLapId = p.NguoiLapId,
        TenNguoiLap = tenNguoiLap,
        NgayTao = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
