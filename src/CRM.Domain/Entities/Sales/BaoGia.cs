using CRM.Domain.Common;

namespace CRM.Domain.Entities.Sales;

public class BaoGia : AuditableEntity<ulong>
{
    public string MaBaoGia { get; set; } = string.Empty;
    public ulong KhachHangId { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = Enums.QuoteStatus.Nhap;
    public uint? NhanVienId { get; set; }
}