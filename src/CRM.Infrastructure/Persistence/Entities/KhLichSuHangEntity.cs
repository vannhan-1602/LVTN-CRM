using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_LichSuHang")]
public class KhLichSuHangEntity
{
    public ulong Id { get; set; }
    public ulong KhachHang_Id { get; set; }
    public ushort? HangCu_Id { get; set; }
    public ushort HangMoi_Id { get; set; }
    public string LyDo { get; set; } = "TuDongDuDieuKien";
    public string? GhiChu { get; set; }
    public uint? NguoiThucHien_Id { get; set; }
    public DateTime? CreatedAt { get; set; }
}
