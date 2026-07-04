using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_DiemThuong")]
public class KhDiemThuongEntity
{
    public ulong Id { get; set; }
    public ulong KhachHang_Id { get; set; }
    public int SoDiem { get; set; }                    // dương = cộng, âm = trừ
    public string LoaiGiaoDich { get; set; } = "MuaHang"; // MuaHang | DoiVoucher | ThuCong
    public ulong? HoaDon_Id { get; set; }
    public ulong? PhieuThuChi_Id { get; set; }
    public DateOnly NgayPhatSinh { get; set; }         // dùng lọc cửa sổ 12 tháng
    public string? GhiChu { get; set; }
    public uint? NguoiTao_Id { get; set; }
    public DateTime? CreatedAt { get; set; }
}
