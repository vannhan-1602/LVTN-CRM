namespace CRM.Application.Interfaces.Email;


// Service gửi email thật qua SMTP/MailKit.
// Tất cả method đều ghi log vào KH_EmailLog sau khi gửi.
public interface IEmailService
{

    // Gửi email xác nhận thanh toán + số điểm vừa được cộng.
    // Gửi ngay sau khi tạo PhieuThuChi thành công.
    Task GuiEmailXacNhanThanhToanAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string maHoaDon, decimal soTienThu, int diemVuaCong, int tongDiem12Thang,
        CancellationToken ct = default);


    // Gửi email thông báo thăng hạng + voucher (nếu có).
    Task GuiEmailThangHangAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenHangCu, string tenHangMoi, string moTaQuyenLoi,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default);

    // Gửi email xuống hạng.
    Task GuiEmailXuongHangAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenHangCu, string tenHangMoi,
        int diemHienTai, int diemCanDat,
        CancellationToken ct = default);


    // Gửi email chúc mừng sinh nhật + voucher (nếu đủ hạng).
    Task GuiEmailSinhNhatAsync(
        ulong khachHangId, string tenKhachHang, string email,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default);

    //Gửi email chúc mừng ngày thành lập công ty (B2B).
    Task GuiEmailNgayThanhLapAsync(
        ulong khachHangId, string tenKhachHang, string email,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default);

    // Gửi email ưu đãi ngày lễ.
    Task GuiEmailNgayLeAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenNgayLe,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default);

    // Gửi email cảnh báo sắp xuống hạng (khi điểm 12 tháng sắp dưới ngưỡng).
    Task GuiEmailCanhBaoXuongHangAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenHangHienTai, int diemHienTai, int diemCanGiu,
        CancellationToken ct = default);

    // Gửi email báo giá kèm link công khai để khách xem & chấp nhận/từ chối
    // Trả về (ThanhCong, LoiChiTiet): GuiAsync tự bắt lỗi SMTP nội bộ và KHÔNG ném exception
    Task<(bool ThanhCong, string? LoiChiTiet)> GuiEmailBaoGiaAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string maBaoGia, decimal tongTien, string quoteLink,
        CancellationToken ct = default);

    // Nhắc thanh toán — gửi khi 1 đợt trong lịch trả góp (HD_LichThanhToan) sắp đến hạn.
    Task GuiEmailNhacThanhToanAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string maHopDong, int soDot, decimal soTien, DateOnly hanThanhToan,
        CancellationToken ct = default);

    // Quá hạn thanh toán — gửi khi 1 đợt trong lịch trả góp đã trễ hạn mà chưa thu.
    Task GuiEmailQuaHanThanhToanAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string maHopDong, int soDot, decimal soTien, DateOnly hanThanhToan,
        CancellationToken ct = default);

    // Nhắc gia hạn hợp đồng — gửi khi hợp đồng sắp hết hạn (mốc 60/30/7 ngày trước NgayKetThuc).
    // Khác với GuiEmailNhacThanhToanAsync (nhắc 1 đợt trả góp) — đây là cảnh báo cấp hợp đồng.
    Task GuiEmailNhacGiaHanHopDongAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string maHopDong, DateOnly ngayKetThuc, int soNgayConLai,
        CancellationToken ct = default);
}