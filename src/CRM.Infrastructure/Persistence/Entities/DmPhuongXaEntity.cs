using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("DM_PhuongXa")]
public class DmPhuongXaEntity
{
    public uint Id { get; set; }
    public uint TinhThanh_Id { get; set; }
    public string TenPhuongXa { get; set; } = string.Empty;

    [ForeignKey("TinhThanh_Id")]
    public DmTinhThanhEntity? TinhThanh { get; set; }
}