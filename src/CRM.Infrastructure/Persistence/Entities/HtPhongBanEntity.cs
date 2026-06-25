using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HT_PhongBan")]
public class HtPhongBanEntity
{
    public ushort Id { get; set; }
    public string TenPhongBan { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<HtThongTinNhanSuEntity> NhanSus { get; set; } = [];
}