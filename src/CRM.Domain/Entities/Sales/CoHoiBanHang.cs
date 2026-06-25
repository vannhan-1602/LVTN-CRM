using CRM.Domain.Common;

namespace CRM.Domain.Entities.Sales;


/// GiaiDoan lưu string khớp enum DB: KhaoSat|DeXuat|ThuongLuong|ThanhCong|ThatBai.

public class CoHoiBanHang : SoftDeletableEntity<ulong>
{
    public string TenThuongVu { get; set; } = string.Empty;
    public string GiaiDoan { get; set; } = "KhaoSat";
    public ulong? KhachHangId { get; set; }
    public ulong? LeadId { get; set; }
    public int TyLeThanhCong { get; set; }           // 0-100 (%)
    public decimal? DoanhThuKyVong { get; set; }
    public string? GhiChu { get; set; }
    public DateOnly? NgayDuKien { get; set; }
    public int? NhanVienPhuTrachId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}