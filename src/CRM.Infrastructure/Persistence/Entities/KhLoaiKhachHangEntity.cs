using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_LoaiKhachHang")]
public class KhLoaiKhachHangEntity
{
    public ushort Id { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool IsActive { get; set; } = true;
}