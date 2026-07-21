using System;

namespace CRM.Domain.Entities.Sales;

public class HopDong
{
    public ulong Id { get; set; }
    public string MaHopDong { get; set; } = string.Empty;
    public ulong KhachHangId { get; set; }
    public ulong? BaoGiaGocId { get; set; }
    public DateOnly? NgayKy { get; set; }
    public int? ThoiHan { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public string HinhThucThanhToan { get; set; } = "ThanhToanMotLan";
    public string TrangThai { get; set; } = "DangThucHien";
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}