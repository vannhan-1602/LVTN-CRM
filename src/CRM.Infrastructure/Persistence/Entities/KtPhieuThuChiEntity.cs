using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KT_PhieuThuChi")]
public class KtPhieuThuChiEntity
{
    public ulong Id { get; set; }
    public string MaPhieu { get; set; } = string.Empty;
    public string LoaiPhieu { get; set; } = "Thu";   // Thu | Chi
    public ulong? KhachHang_Id { get; set; }
    public ulong? HoaDon_Id { get; set; }
    public decimal SoTien { get; set; }
    public uint? NguoiLap_Id { get; set; }            // int unsigned trong DB → uint? trong C#
    public DateTime? NgayTao { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
