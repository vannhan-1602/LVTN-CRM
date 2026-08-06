namespace CRM.Application.Features.Contracts.DTOs;

// ─────────────────────────────────────────────────────────────────────────
// Toàn bộ dữ liệu "trình bày" của bản in hợp đồng — chính là những gì người
// dùng đã tinh chỉnh trên ContractPrintPage (bên A, bên B, các tham số, nội
// dung từng điều khoản). KHÔNG có bảng nào trong DB lưu các trường này theo
// thiết kế (xem README ở ContractPrintPage.jsx) — nên khi cần gửi email kèm
// PDF, các trường này phải được gửi kèm trong request thay vì đọc lại từ DB.
// Các số liệu có tính pháp lý/tiền bạc (mã hợp đồng, giá trị, khách hàng...)
// KHÔNG lấy từ đây — handler tự truy vấn lại từ DB để đảm bảo đúng dữ liệu
// thật, tránh trường hợp người dùng sửa số tiền trên trình duyệt rồi gửi đi.
// ─────────────────────────────────────────────────────────────────────────

public class ContractPartyInfoDto
{
    public string? TenCongTy { get; set; }
    public string? DiaChi { get; set; }
    public string? MaSoThue { get; set; }
    public string? GiayDkkd { get; set; }
    public string? DienThoai { get; set; }
    public string? Email { get; set; }
    public string? SoTaiKhoan { get; set; }
    public string? NganHang { get; set; }
    public string? NguoiDaiDien { get; set; }
    public string? ChucVu { get; set; }
    public string? Cccd { get; set; }
}

public class ContractClauseTextsDto
{
    public string? Dieu3Cham { get; set; }
    public string? Dieu4 { get; set; }
    public string? Dieu5 { get; set; }
    public string? Dieu6 { get; set; }
    public string? Dieu7 { get; set; }
    public string? Dieu8 { get; set; }
    public string? Dieu9 { get; set; }
    public string? Dieu10 { get; set; }
    public string? Dieu11 { get; set; }
    public string? Dieu12 { get; set; }
    public string? Dieu13 { get; set; }
    public string? Dieu14 { get; set; }
}

public class SendContractEmailRequestDto
{
    public ContractPartyInfoDto BenA { get; set; } = new();
    public ContractPartyInfoDto BenB { get; set; } = new();
    public string? DiaDiemKy { get; set; }
    public bool VatIncluded { get; set; } = true;
    public int BaoHanhThang { get; set; } = 12;
    public decimal MucPhatViPham { get; set; } = 8;
    public int SoBan { get; set; } = 2;
    public ContractClauseTextsDto ClauseTexts { get; set; } = new();

    /// <summary>Lời nhắn thêm của nhân viên gửi kèm email, hiển thị phía trên nút xem hợp đồng.</summary>
    public string? LoiNhan { get; set; }
}

public class SendContractEmailResultDto
{
    public bool ThanhCong { get; set; }
    public string? LoiChiTiet { get; set; }
    public DateTime ThoiGianGui { get; set; }
    public string EmailDaGui { get; set; } = string.Empty;
}

public class ContractEmailHistoryItemDto
{
    public DateTime? CreatedAt { get; set; }
    public bool ThanhCong { get; set; }
    public string EmailDen { get; set; } = string.Empty;
    public string? LoiChiTiet { get; set; }
}
