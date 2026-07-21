using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HD_LichThanhToan")]
public class HdLichThanhToanEntity
{
    public ulong Id { get; set; }
    public ulong HopDong_Id { get; set; }
    public int SoDot { get; set; }
    public decimal SoTien { get; set; }
    public DateOnly HanThanhToan { get; set; }
    public string TrangThai { get; set; } = "ChuaDenHan";

    [ForeignKey("HopDong_Id")]
    public HdHopDongEntity? HopDong { get; set; }
}