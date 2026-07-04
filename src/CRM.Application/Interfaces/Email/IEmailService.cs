namespace CRM.Application.Interfaces.Email;

/// <summary>
/// Service gửi email thật qua SMTP/MailKit.
/// Tất cả method đều ghi log vào KH_EmailLog sau khi gửi.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email xác nhận thanh toán + số điểm vừa được cộng.
    /// Gửi ngay sau khi tạo PhieuThuChi thành công.
    /// </summary>
    Task GuiEmailXacNhanThanhToanAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string maHoaDon, decimal soTienThu, int diemVuaCong, int tongDiem12Thang,
        CancellationToken ct = default);

    /// <summary>
    /// Gửi email thông báo thăng hạng + voucher (nếu có).
    /// </summary>
    Task GuiEmailThangHangAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenHangCu, string tenHangMoi, string moTaQuyenLoi,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default);

    /// <summary>
    /// Gửi email xuống hạng.
    /// </summary>
    Task GuiEmailXuongHangAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenHangCu, string tenHangMoi,
        int diemHienTai, int diemCanDat,
        CancellationToken ct = default);

    /// <summary>
    /// Gửi email chúc mừng sinh nhật + voucher (nếu đủ hạng).
    /// </summary>
    Task GuiEmailSinhNhatAsync(
        ulong khachHangId, string tenKhachHang, string email,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default);

    /// <summary>
    /// Gửi email chúc mừng ngày thành lập công ty (B2B).
    /// </summary>
    Task GuiEmailNgayThanhLapAsync(
        ulong khachHangId, string tenKhachHang, string email,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default);

    /// <summary>
    /// Gửi email ưu đãi ngày lễ.
    /// </summary>
    Task GuiEmailNgayLeAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenNgayLe,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default);

    /// <summary>
    /// Gửi email cảnh báo sắp xuống hạng (khi điểm 12 tháng sắp dưới ngưỡng).
    /// </summary>
    Task GuiEmailCanhBaoXuongHangAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenHangHienTai, int diemHienTai, int diemCanGiu,
        CancellationToken ct = default);
}
