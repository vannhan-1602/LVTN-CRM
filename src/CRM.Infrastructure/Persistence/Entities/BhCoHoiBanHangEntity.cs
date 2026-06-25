using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("BH_CoHoiBanHang")]
public class BhCoHoiBanHangEntity
{
    public ulong Id { get; set; }
    public string TenThuongVu { get; set; } = string.Empty;
    public string GiaiDoan { get; set; } = "KhaoSat";
    public ulong? KhachHang_Id { get; set; }
    public ulong? Lead_Id { get; set; }
    public int TyLeThanhCong { get; set; }
    public decimal? DoanhThuKyVong { get; set; }
    public string? GhiChu { get; set; }
    public DateOnly? NgayDuKien { get; set; }
    public int? NhanVienPhuTrach_Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}