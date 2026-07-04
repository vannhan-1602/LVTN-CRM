using CRM.Domain.Common;

namespace CRM.Domain.Entities.Loyalty;

public class DiemThuong : BaseEntity<ulong>
{
    public ulong KhachHangId { get; set; }
    public int SoDiem { get; set; }
    public string LoaiGiaoDich { get; set; } = "MuaHang";
    public ulong? HoaDonId { get; set; }
    public ulong? PhieuThuChiId { get; set; }
    public DateOnly NgayPhatSinh { get; set; }
    public string? GhiChu { get; set; }
    public uint? NguoiTaoId { get; set; }
    public DateTime? CreatedAt { get; set; }
}
