using CRM.Application.Features.Opportunities.DTOs;
using CRM.Domain.Entities.Sales;

namespace CRM.Application.Features.Opportunities.Mappings;

public static class OpportunityMapper
{
    public static OpportunityDto ToDto(CoHoiBanHang d,
        string? tenKhachHang = null,
        string? tenLead = null,
        string? tenNhanVien = null) => new()
        {
            Id = d.Id,
            TenThuongVu = d.TenThuongVu,
            GiaiDoan = d.GiaiDoan,
            KhachHangId = d.KhachHangId,
            TenKhachHang = tenKhachHang,
            LeadId = d.LeadId,
            TenLead = tenLead,
            TyLeThanhCong = d.TyLeThanhCong,
            DoanhThuKyVong = d.DoanhThuKyVong,
            GhiChu = d.GhiChu,
            NgayDuKien = d.NgayDuKien,
            NhanVienPhuTrachId = d.NhanVienPhuTrachId,
            TenNhanVien = tenNhanVien,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt,
        };

    public static CoHoiBanHang ToDomain(
        string tenThuongVu, ulong? khachHangId, ulong? leadId,
        int tyLeThanhCong, decimal? doanhThuKyVong,
        string? ghiChu, DateOnly? ngayDuKien,
        int? nhanVienPhuTrachId,
        string giaiDoan = "KhaoSat") => new()
        {
            TenThuongVu = tenThuongVu,
            GiaiDoan = giaiDoan,
            KhachHangId = khachHangId,
            LeadId = leadId,
            TyLeThanhCong = tyLeThanhCong,
            DoanhThuKyVong = doanhThuKyVong,
            GhiChu = ghiChu,
            NgayDuKien = ngayDuKien,
            NhanVienPhuTrachId = nhanVienPhuTrachId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
}