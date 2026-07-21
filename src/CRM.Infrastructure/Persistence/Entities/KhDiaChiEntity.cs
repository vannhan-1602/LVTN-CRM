using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_DiaChi")]
public class KhDiaChiEntity
{
    public ulong Id { get; set; }
    public ulong KhachHang_Id { get; set; }
    public string? DiaChiChiTiet { get; set; }
    public uint? TinhThanh_Id { get; set; }
    public uint? PhuongXa_Id { get; set; }
    public string? LoaiDiaChi { get; set; }
    public bool IsDefault { get; set; }

    [ForeignKey("TinhThanh_Id")]
    public DmTinhThanhEntity? TinhThanh { get; set; }

    [ForeignKey("PhuongXa_Id")]
    public DmPhuongXaEntity? PhuongXa { get; set; }
}