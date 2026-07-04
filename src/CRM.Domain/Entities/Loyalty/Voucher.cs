using CRM.Domain.Common;

namespace CRM.Domain.Entities.Loyalty;

public class Voucher : BaseEntity<ulong>
{
    public string MaVoucher { get; set; } = string.Empty;
    public ulong KhachHangId { get; set; }
    public string LoaiGiamGia { get; set; } = "PhanTram";
    public decimal GiaTriGiam { get; set; }
    public decimal? GiaTriGiamToiDa { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayHetHan { get; set; }
    public string LyDoPhatHanh { get; set; } = "ThangHang";
    public ulong? LichSuHangId { get; set; }
    public string TrangThaiYeuCau { get; set; } = "ChuaYeuCau";
    public ulong? TicketId { get; set; }
    public bool IsUsed { get; set; } = false;
    public ulong? AppliedToBaoGiaId { get; set; }
    public DateTime? NgaySuDung { get; set; }
    public uint? NguoiApDungId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
