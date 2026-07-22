using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HD_HopDong")]
public class HdHopDongEntity
{
    public ulong Id { get; set; }
    public string MaHopDong { get; set; } = string.Empty;
    public ulong KhachHangId { get; set; }
    public ulong? BaoGiaId { get; set; }
    public DateOnly? NgayKy { get; set; }
    public int? ThoiHan { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public string HinhThucThanhToan { get; set; } = "ThanhToanMotLan";
    public string TrangThai { get; set; } = "DangThucHien";
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>ChinhThuc | GiaHan | BaoTri — hợp đồng gia hạn/bảo trì tự tham chiếu qua HopDongGocId.</summary>
    public string LoaiHopDong { get; set; } = "ChinhThuc";
    public ulong? HopDongGocId { get; set; }
    public DateOnly? NgayNhacGiaHanCuoi { get; set; }

    public ICollection<HdLichThanhToanEntity> LichThanhToans { get; set; } = new List<HdLichThanhToanEntity>();
}