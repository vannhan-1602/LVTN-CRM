using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_XepHang")]
public class KhXepHangEntity
{
    public ushort Id { get; set; }
    public string MaHang { get; set; } = string.Empty;
    public string TenHang { get; set; } = string.Empty;
    public byte ThuTu { get; set; }
    public int DiemToiThieu { get; set; }
    public int SoLanThuToiThieu { get; set; }
    public decimal PhanTramGiamVoucher { get; set; }
    public string? MoTaQuyenLoi { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
