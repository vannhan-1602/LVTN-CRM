using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HD_License")]
public class HdLicenseEntity
{
    public ulong Id { get; set; }
    public ulong HopDong_Id { get; set; }
    public uint SanPham_Id { get; set; }
    public int SoLuongUser { get; set; } = 1;
    public string? PhienBan { get; set; }
    public string? MaLicenseKey { get; set; }
    public string MoiTruongTrienKhai { get; set; } = "Cloud";
    public DateOnly? NgayKichHoat { get; set; }
    public DateOnly? NgayHetHan { get; set; }
    public string TrangThai { get; set; } = "DangHoatDong";
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("HopDong_Id")]
    public HdHopDongEntity? HopDong { get; set; }

    [ForeignKey("SanPham_Id")]
    public BhSanPhamEntity? SanPham { get; set; }
}
