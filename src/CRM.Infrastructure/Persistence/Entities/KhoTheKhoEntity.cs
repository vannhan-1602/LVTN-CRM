using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("Kho_TheKho")]
public class KhoTheKhoEntity
{
    public ulong Id { get; set; }
    public uint SanPham_Id { get; set; }
    public string? MaChungTu { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public int SoLuongThayDoi { get; set; }
    public int TonCuoi { get; set; }
    public DateTime NgayGiaoDich { get; set; }
    public uint? NguoiThucHien_Id { get; set; }
    public string? GhiChu { get; set; }
}