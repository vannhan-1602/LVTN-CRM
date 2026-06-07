using CRM.Domain.Common;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities.Sales;

public class CoHoiBanHang : SoftDeletableEntity<ulong>
{
    public string TenThuongVu { get; set; } = string.Empty;
    public string GiaiDoan { get; set; } = CoHoiGiaiDoan.KhaoSat.ToString();
    public ulong? KhachHangId { get; set; }
    public ulong? LeadId { get; set; }
    public int TyLeThanhCong { get; set; }
    public decimal? DoanhThuKyVong { get; set; }
    public string? GhiChu { get; set; }
    public DateOnly? NgayDuKien { get; set; }
    public uint? NhanVienPhuTrachId { get; set; }
}
