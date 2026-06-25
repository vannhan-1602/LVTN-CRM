namespace CRM.Application.Features.Opportunities.DTOs;

public class OpportunityDto
{
    public ulong Id { get; set; }
    public string TenThuongVu { get; set; } = string.Empty;
    public string GiaiDoan { get; set; } = string.Empty;
    public ulong? KhachHangId { get; set; }
    public string? TenKhachHang { get; set; }
    public ulong? LeadId { get; set; }
    public string? TenLead { get; set; }
    public int TyLeThanhCong { get; set; }
    public decimal? DoanhThuKyVong { get; set; }
    public string? GhiChu { get; set; }
    public DateOnly? NgayDuKien { get; set; }
    public int? NhanVienPhuTrachId { get; set; }
    public string? TenNhanVien { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateOpportunityRequestDto
{
    public string TenThuongVu { get; set; } = string.Empty;
    public ulong? KhachHangId { get; set; }
    public ulong? LeadId { get; set; }
    public int TyLeThanhCong { get; set; }
    public decimal? DoanhThuKyVong { get; set; }
    public string? GhiChu { get; set; }
    public DateOnly? NgayDuKien { get; set; }
}

public class UpdateOpportunityRequestDto
{
    public string TenThuongVu { get; set; } = string.Empty;
    public ulong? KhachHangId { get; set; }
    public ulong? LeadId { get; set; }
    public int TyLeThanhCong { get; set; }
    public decimal? DoanhThuKyVong { get; set; }
    public string? GhiChu { get; set; }
    public DateOnly? NgayDuKien { get; set; }
}

public class ChangeStageRequestDto
{
    public string GiaiDoan { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
}