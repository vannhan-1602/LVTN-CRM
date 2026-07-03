using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_NgayLe")]
public class KhNgayLeEntity
{
    public ushort Id { get; set; }
    public string TenNgayLe { get; set; } = string.Empty;
    public byte Thang { get; set; }
    public byte Ngay { get; set; }
    public byte SoNgayGuiTruoc { get; set; } = 5;
    public string ApDungChoLoaiKH { get; set; } = "TatCa";
    public ushort? HangToiThieuApDung { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
}
