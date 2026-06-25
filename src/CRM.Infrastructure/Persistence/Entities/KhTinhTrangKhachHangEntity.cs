using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_TinhTrangKhachHang")]
public class KhTinhTrangKhachHangEntity
{
    public ushort Id { get; set; }
    public string TenTinhTrang { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}