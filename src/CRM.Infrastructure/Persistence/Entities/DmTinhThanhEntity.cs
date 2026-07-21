using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("DM_TinhThanh")]
public class DmTinhThanhEntity
{
    public uint Id { get; set; }
    public string TenTinhThanh { get; set; } = string.Empty;
    public ICollection<DmPhuongXaEntity> PhuongXas { get; set; } = new List<DmPhuongXaEntity>();
}