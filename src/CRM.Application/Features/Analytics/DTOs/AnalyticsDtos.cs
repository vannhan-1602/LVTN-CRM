namespace CRM.Application.Features.Analytics.DTOs;

/// <summary>Dữ liệu tổng hợp gửi cho AI phân tích — số liệu thô, không phải văn bản.</summary>
public class SalesAnalyticsDataDto
{
    public int SoThangPhanTich { get; set; }
    public List<DoanhThuThangDto> DoanhThuTheoThang { get; set; } = new();
    public int TongSoCoHoi { get; set; }
    public int SoCoHoiThanhCong { get; set; }
    public int SoCoHoiThatBai { get; set; }
    public decimal TyLeThangCoHoi { get; set; } // %
    public List<SanPhamBanChayDto> Top5SanPhamBanChay { get; set; } = new();
    public int TongSoTicket { get; set; }
    public int SoTicketDangMo { get; set; }
    public int SoTicketKhanCap { get; set; }
    public decimal TongCongNoChuaThu { get; set; }
}

public class DoanhThuThangDto
{
    public int Nam { get; set; }
    public int Thang { get; set; }
    public decimal DoanhThu { get; set; }
    public int SoHoaDon { get; set; }
}

public class SanPhamBanChayDto
{
    public uint SanPhamId { get; set; }
    public string TenSanPham { get; set; } = string.Empty;
    public int SoLuongBan { get; set; }
}

/// <summary>Tổng hợp Phiếu Chi (chi phí phát sinh cho khách hàng — ăn uống, đàm phán...),
/// KHÔNG liên quan tới tiến độ thanh toán hóa đơn/hợp đồng. Dùng cho Dashboard quản lý.</summary>
public class ChiSummaryDto
{
    public decimal TongChiThangNay { get; set; }
    public int SoPhieuChiThangNay { get; set; }
    public List<ChiTheoKhachHangDto> TopKhachHangPhatSinhChi { get; set; } = new();
}

public class ChiTheoKhachHangDto
{
    public ulong KhachHangId { get; set; }
    public string TenKhachHang { get; set; } = string.Empty;
    public decimal TongChi { get; set; }
    public int SoPhieu { get; set; }
}

/// <summary>So sánh số bản ghi mới tạo tháng này với tháng trước — dùng cho mũi tên xu hướng trên Dashboard.</summary>
public class DashboardTrendsDto
{
    public int KhachHangMoiThangNay { get; set; }
    public int KhachHangMoiThangTruoc { get; set; }
    public int HopDongMoiThangNay { get; set; }
    public int HopDongMoiThangTruoc { get; set; }
    public int BaoGiaMoiThangNay { get; set; }
    public int BaoGiaMoiThangTruoc { get; set; }
    public int TicketMoiThangNay { get; set; }
    public int TicketMoiThangTruoc { get; set; }
}

/// <summary>Kết quả trả về cho frontend — số liệu thô (để vẽ biểu đồ) + đề xuất có cấu trúc từ AI.</summary>
public class AiSalesAnalysisResultDto
{
    public DateTime GeneratedAt { get; set; }
    public SalesAnalyticsDataDto DuLieu { get; set; } = null!;
    public string NhanDinhTongQuan { get; set; } = string.Empty;
    public List<AiDeXuatDto> DeXuat { get; set; } = new();
}

/// <summary>1 đề xuất kiểu "agency" — có tiêu đề, mô tả ngắn, mức ưu tiên, nhóm vấn đề (để gắn icon/màu).</summary>
public class AiDeXuatDto
{
    public string TieuDe { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public string MucDoUuTien { get; set; } = "TrungBinh"; // Cao | TrungBinh | Thap
    public string NhomVanDe { get; set; } = "Khac"; // DoanhThu | CoHoi | CongNo | Ticket | SanPham | Khac
}
