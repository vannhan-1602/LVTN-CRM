using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HD_MocTrienKhai")]
public class HdMocTrienKhaiEntity
{
    public ulong Id { get; set; }
    public ulong HopDong_Id { get; set; }

    /// <summary>DaoTao | BanGiao | NghiemThu</summary>
    public string LoaiMoc { get; set; } = string.Empty;
    public string? NoiDung { get; set; }
    public DateTime? NgayThucHien { get; set; }
    public uint? NhanVienThucHien_Id { get; set; }
    public string? NguoiXacNhanKhach { get; set; }
    public string? FileBienBan { get; set; }
    public string TrangThai { get; set; } = "ChuaThucHien";
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("HopDong_Id")]
    public HdHopDongEntity? HopDong { get; set; }

    [ForeignKey("NhanVienThucHien_Id")]
    public HtUserEntity? NhanVienThucHien { get; set; }
}
