namespace CRM.Application.Features.Quotes.DTOs;

public class QuoteDto
{
    public ulong Id { get; set; }
    public string MaBaoGia { get; set; } = string.Empty;
    public ulong KhachHangId { get; set; }
    public string? TenKhachHang { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public uint? NhanVienId { get; set; }
    public string? TenNhanVien { get; set; }
    public string? LyDoTuChoi { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Không lưu DB — chỉ set tạm sau khi gọi SendQuoteCommand, để Controller/Frontend
    // biết email THẬT SỰ có gửi được hay không (tránh báo "Đã gửi" chung chung dù
    // email bị bỏ qua do khách chưa có địa chỉ email hoặc SMTP lỗi).
    public bool? EmailDaGui { get; set; }
    public string? EmailLyDoKhongGui { get; set; }
}

public class QuoteDetailItemDto
{
    public ulong Id { get; set; }
    public uint SanPhamId { get; set; }
    public string? TenSP { get; set; }
    public string? MaSP { get; set; }
    public string? DonVi { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SoLuong * DonGia;
}

public class QuoteDetailDto : QuoteDto
{
    public List<QuoteDetailItemDto> ChiTiet { get; set; } = [];
}

public class QuoteItemRequestDto
{
    public uint SanPhamId { get; set; }
    public int SoLuong { get; set; }
    public decimal? DonGia { get; set; } // null = lấy GiaBan hiện tại của sản phẩm
}

public class CreateQuoteRequestDto
{
    public ulong KhachHangId { get; set; }
    public List<QuoteItemRequestDto> ChiTiet { get; set; } = [];
}

public class UpdateQuoteRequestDto
{
    public List<QuoteItemRequestDto> ChiTiet { get; set; } = [];
}

public class RejectQuoteRequestDto
{
    public string? LyDo { get; set; }
}
