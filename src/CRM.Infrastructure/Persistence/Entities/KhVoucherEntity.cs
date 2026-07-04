using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_Voucher")]
public class KhVoucherEntity
{
    public ulong Id { get; set; }
    public string MaVoucher { get; set; } = string.Empty;
    public ulong KhachHang_Id { get; set; }
    public string LoaiGiamGia { get; set; } = "PhanTram";   // PhanTram | SoTienCoDinh
    public decimal GiaTriGiam { get; set; }
    public decimal? GiaTriGiamToiDa { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayHetHan { get; set; }
    public string LyDoPhatHanh { get; set; } = "ThangHang"; // ThangHang|SinhNhat|NgayThanhLap|NgayLe|CuoiNam|ThuCong
    public ulong? LichSuHang_Id { get; set; }
    public string TrangThaiYeuCau { get; set; } = "ChuaYeuCau"; // ChuaYeuCau | DaYeuCau
    public ulong? Ticket_Id { get; set; }
    public bool IsUsed { get; set; } = false;
    public ulong? AppliedTo_BaoGia_Id { get; set; }
    public DateTime? NgaySuDung { get; set; }
    public uint? NguoiApDung_Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
