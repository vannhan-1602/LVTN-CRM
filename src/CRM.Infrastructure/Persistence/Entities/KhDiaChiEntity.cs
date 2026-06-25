using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_DiaChi")]
public class KhDiaChiEntity
{
    public ulong Id { get; set; }
    public ulong KhachHang_Id { get; set; }
    public string? DiaChiChiTiet { get; set; }
    public string? TinhThanh { get; set; }
    public string? QuanHuyen { get; set; }
    public string? PhuongXa { get; set; }
    public string? LoaiDiaChi { get; set; }
    public bool IsDefault { get; set; }
}