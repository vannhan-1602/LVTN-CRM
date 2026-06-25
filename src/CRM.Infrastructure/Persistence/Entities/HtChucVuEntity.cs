using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HT_ChucVu")]
public class HtChucVuEntity
{
    public ushort Id { get; set; }
    public string TenChucVu { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<HtThongTinNhanSuEntity> NhanSus { get; set; } = [];
}