

using CRM.Application.Features.Vouchers.DTOs;

namespace CRM.Application.Features.Invoices.DTOs;

public class InvoiceDto
{
    public ulong Id { get; set; }
    public string MaHoaDon { get; set; } = string.Empty;
    public ulong HopDongId { get; set; }
    public string? MaHopDong { get; set; }
    public ulong KhachHangId { get; set; }
    public string? TenKhachHang { get; set; }
    public decimal TongTien { get; set; }
    public decimal SoTienDaThu { get; set; }
    public decimal SoTienConLai => TongTien - SoTienDaThu;
    public string TrangThaiThanhToan { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<VoucherDto> PhieuThuChi { get; set; } = [];
}

public class CreateInvoiceRequestDto
{
    public ulong HopDongId { get; set; }
    public decimal TongTien { get; set; }
}
