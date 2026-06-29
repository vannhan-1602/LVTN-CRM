using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("TK_Ticket")]
public class TkTicketEntity
{
    public ulong Id { get; set; }
    public string MaTicket { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string? FileDinhKem { get; set; }
    public ushort? LoaiTicket_Id { get; set; }
    public ulong KhachHang_Id { get; set; }
    public ulong? HopDongId { get; set; }
    public uint? SanPham_Id { get; set; }
    public string MucDoUuTien { get; set; } = "TrungBinh";
    public string NguonTiepNhan { get; set; } = "Phone";
    public string TrangThai { get; set; } = "Moi";
    public uint? NhanVienTiepNhan_Id { get; set; }
    public uint? NhanVienXuLy_Id { get; set; }
    public DateTime? NgayHenXuLy { get; set; }
    public DateTime? NgayDong { get; set; }
    public string? LyDoDong { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}