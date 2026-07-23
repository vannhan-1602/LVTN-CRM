namespace CRM.Application.Features.Invoices.DTOs;

public class InvoiceDto
{
    public ulong Id { get; set; }
    public string MaHoaDon { get; set; } = string.Empty;
    public ulong? HopDongId { get; set; }
    public string? MaHopDong { get; set; }
    public ulong? LichThanhToanId { get; set; }
    public int? SoDot { get; set; }
    public ulong KhachHangId { get; set; }
    public string? TenKhachHang { get; set; }
    public uint? NhanVienPhuTrachId { get; set; }
    public decimal TongTien { get; set; }
    public decimal SoTienDaThu { get; set; }
    public decimal SoTienConLai => TongTien - SoTienDaThu;
    public string TrangThaiThanhToan { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateInvoiceRequestDto
{
    /// <summary>
    /// Hợp đồng gốc (nếu có). Khi truyền HopDongId, hệ thống tự lấy KhachHangId
    /// từ hợp đồng — không cần truyền KhachHangId riêng.
    /// Hợp đồng phải đang ở trạng thái DangThucHien.
    /// </summary>
    public ulong? HopDongId { get; set; }

    /// <summary>
    /// Bắt buộc nếu không có HopDongId (hóa đơn bán lẻ không qua hợp đồng).
    /// </summary>
    public ulong? KhachHangId { get; set; }

    /// <summary>
    /// Bắt buộc nếu hợp đồng thuộc dạng trả góp (HinhThucThanhToan = "TraGop") — đợt trong
    /// HD_LichThanhToan mà hóa đơn này ứng với. Bỏ trống nếu thanh toán 1 lần hoặc bán lẻ.
    /// </summary>
    public ulong? LichThanhToanId { get; set; }

    public decimal TongTien { get; set; }
}
