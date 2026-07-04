namespace CRM.Infrastructure.Services.Email;

/// <summary>
/// Tạo HTML template cho các loại email.
/// Style đơn giản, tương thích Gmail/Outlook, không phụ thuộc framework CSS.
/// </summary>
internal static class EmailTemplateHelper
{
    private const string BASE_COLOR = "#2563EB";   // accent blue
    private const string SUCCESS_COLOR = "#16A34A";
    private const string WARNING_COLOR = "#D97706";
    private const string DANGER_COLOR  = "#DC2626";

    private static string WrapLayout(string tenKhachHang, string heading, string body, string footer = "") => $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#F1F5F9;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#F1F5F9;padding:32px 16px;">
            <tr><td align="center">
              <table width="560" cellpadding="0" cellspacing="0" style="background:#FFFFFF;border-radius:12px;overflow:hidden;box-shadow:0 1px 4px rgba(0,0,0,.08);">
                <!-- Header -->
                <tr><td style="background:{BASE_COLOR};padding:24px 32px;">
                  <p style="margin:0;color:#FFFFFF;font-size:20px;font-weight:700;letter-spacing:.5px;">CRM System</p>
                </td></tr>
                <!-- Body -->
                <tr><td style="padding:32px;">
                  <p style="margin:0 0 8px;font-size:14px;color:#64748B;">Kính gửi,</p>
                  <p style="margin:0 0 20px;font-size:16px;font-weight:600;color:#0F172A;">{tenKhachHang}</p>
                  <h2 style="margin:0 0 16px;font-size:22px;font-weight:700;color:#0F172A;">{heading}</h2>
                  {body}
                </td></tr>
                <!-- Footer -->
                <tr><td style="padding:16px 32px 24px;border-top:1px solid #E2E8F0;">
                  <p style="margin:0;font-size:12px;color:#94A3B8;">
                    Email này được gửi tự động từ CRM System. Vui lòng không trả lời email này.<br>
                    {footer}
                  </p>
                </td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    // ── Xác nhận thanh toán + cộng điểm ────────────────────────────────────
    public static string XacNhanThanhToan(
        string tenKhachHang, string maHoaDon,
        decimal soTienThu, int diemVuaCong, int tongDiem) =>
        WrapLayout(tenKhachHang, "Xác nhận thanh toán thành công", $"""
            <p style="color:#334155;line-height:1.6;">
              Chúng tôi xác nhận đã nhận được khoản thanh toán của bạn cho hóa đơn <strong>{maHoaDon}</strong>.
            </p>
            <table style="width:100%;background:#F8FAFC;border-radius:8px;padding:16px;margin:16px 0;border:1px solid #E2E8F0;">
              <tr><td style="color:#64748B;font-size:13px;padding:4px 0;">Số tiền đã thu</td>
                  <td align="right" style="font-weight:700;color:#0F172A;">{soTienThu:N0} VNĐ</td></tr>
              <tr><td style="color:#64748B;font-size:13px;padding:4px 0;">Điểm vừa cộng</td>
                  <td align="right" style="font-weight:700;color:{SUCCESS_COLOR};">+{diemVuaCong} điểm</td></tr>
              <tr style="border-top:1px solid #E2E8F0;">
                  <td style="color:#64748B;font-size:13px;padding:8px 0 4px;">Tổng điểm tích lũy (12T)</td>
                  <td align="right" style="font-weight:700;font-size:18px;color:{BASE_COLOR};padding:8px 0 4px;">{tongDiem} điểm</td></tr>
            </table>
            <p style="color:#64748B;font-size:13px;">
              Điểm tích lũy được tính trong vòng 12 tháng gần nhất và dùng để xác định hạng khách hàng.
            </p>
            """);

    // ── Thăng hạng + voucher ────────────────────────────────────────────────
    public static string ThangHang(
        string tenKhachHang, string tenHangCu, string tenHangMoi,
        string moTaQuyenLoi, string? maVoucher,
        decimal? phanTramGiam, string? voucherLink) =>
        WrapLayout(tenKhachHang, $"🎉 Chúc mừng! Bạn đã lên hạng {tenHangMoi}", $"""
            <p style="color:#334155;line-height:1.6;">
              Tuyệt vời! Nhờ giao dịch tích cực trong 12 tháng qua, bạn đã thăng từ hạng
              <strong>{tenHangCu}</strong> lên hạng <strong style="color:{BASE_COLOR};">{tenHangMoi}</strong>.
            </p>
            <div style="background:#EFF6FF;border-left:4px solid {BASE_COLOR};border-radius:4px;padding:16px;margin:16px 0;">
              <p style="margin:0;font-size:14px;font-weight:600;color:#1E40AF;">Quyền lợi hạng {tenHangMoi}</p>
              <p style="margin:8px 0 0;font-size:13px;color:#334155;">{moTaQuyenLoi}</p>
            </div>
            {(maVoucher is not null ? $"""
            <div style="background:#F0FDF4;border:1px dashed {SUCCESS_COLOR};border-radius:8px;padding:20px;margin:16px 0;text-align:center;">
              <p style="margin:0 0 4px;font-size:13px;color:#16A34A;">🎁 Voucher thăng hạng dành riêng cho bạn</p>
              <p style="margin:0 0 12px;font-size:28px;font-weight:700;color:#0F172A;letter-spacing:2px;">{maVoucher}</p>
              <p style="margin:0 0 16px;font-size:13px;color:#64748B;">Giảm {phanTramGiam}% cho đơn hàng tiếp theo · Hiệu lực 90 ngày</p>
              <a href="{voucherLink}" style="display:inline-block;background:{SUCCESS_COLOR};color:#FFFFFF;text-decoration:none;padding:10px 24px;border-radius:8px;font-weight:600;font-size:14px;">
                Sử dụng voucher ngay
              </a>
            </div>
            <p style="color:#94A3B8;font-size:12px;">
              Sau khi bấm nút trên, đội ngũ của chúng tôi sẽ liên hệ để hỗ trợ bạn sử dụng ưu đãi.
            </p>
            """ : "")}
            """);

    // ── Xuống hạng ──────────────────────────────────────────────────────────
    public static string XuongHang(
        string tenKhachHang, string tenHangCu, string tenHangMoi,
        int diemHienTai, int diemCanDat) =>
        WrapLayout(tenKhachHang, $"Thông báo thay đổi hạng khách hàng", $"""
            <p style="color:#334155;line-height:1.6;">
              Do lượng giao dịch trong 12 tháng gần nhất chưa đạt tiêu chí,
              hạng của bạn đã thay đổi từ <strong>{tenHangCu}</strong>
              xuống <strong>{tenHangMoi}</strong>.
            </p>
            <table style="width:100%;background:#FFF7ED;border-radius:8px;padding:16px;margin:16px 0;border:1px solid #FED7AA;">
              <tr><td style="color:#64748B;font-size:13px;">Điểm hiện tại (12T)</td>
                  <td align="right" style="font-weight:700;color:{WARNING_COLOR};">{diemHienTai} điểm</td></tr>
              <tr><td style="color:#64748B;font-size:13px;">Cần đạt để giữ hạng {tenHangCu}</td>
                  <td align="right" style="font-weight:700;color:#0F172A;">{diemCanDat} điểm</td></tr>
            </table>
            <p style="color:#334155;font-size:13px;">
              Tiếp tục giao dịch để tích lũy điểm và phục hồi hạng trong những tháng tới.
              Mọi thắc mắc vui lòng liên hệ đội ngũ chăm sóc khách hàng.
            </p>
            """);

    // ── Sinh nhật (B2C) ─────────────────────────────────────────────────────
    public static string SinhNhat(
        string tenKhachHang, string? maVoucher,
        decimal? phanTramGiam, string? voucherLink) =>
        WrapLayout(tenKhachHang, "🎂 Chúc mừng sinh nhật!", $"""
            <p style="color:#334155;line-height:1.6;">
              Nhân dịp sinh nhật của bạn, toàn thể đội ngũ chúng tôi xin gửi lời chúc
              sức khoẻ, hạnh phúc và nhiều thành công hơn nữa!
            </p>
            {VoucherBlock(maVoucher, phanTramGiam, voucherLink, "🎁 Quà sinh nhật đặc biệt dành cho bạn")}
            """);

    // ── Kỷ niệm thành lập (B2B) ─────────────────────────────────────────────
    public static string NgayThanhLap(
        string tenKhachHang, string? maVoucher,
        decimal? phanTramGiam, string? voucherLink) =>
        WrapLayout(tenKhachHang, "🏢 Chúc mừng kỷ niệm ngày thành lập!", $"""
            <p style="color:#334155;line-height:1.6;">
              Nhân kỷ niệm ngày thành lập của quý công ty, chúng tôi trân trọng cảm ơn
              sự tin tưởng và hợp tác trong suốt thời gian qua.
            </p>
            {VoucherBlock(maVoucher, phanTramGiam, voucherLink, "🎁 Ưu đãi kỷ niệm dành cho quý công ty")}
            """);

    // ── Ngày lễ ─────────────────────────────────────────────────────────────
    public static string NgayLe(
        string tenKhachHang, string tenNgayLe, string? maVoucher,
        decimal? phanTramGiam, string? voucherLink) =>
        WrapLayout(tenKhachHang, $"🎉 Ưu đãi {tenNgayLe} dành riêng cho bạn", $"""
            <p style="color:#334155;line-height:1.6;">
              Nhân dịp <strong>{tenNgayLe}</strong>, chúng tôi gửi tặng bạn ưu đãi đặc biệt
              tri ân sự đồng hành trong thời gian qua.
            </p>
            {VoucherBlock(maVoucher, phanTramGiam, voucherLink, $"🎁 Ưu đãi {tenNgayLe}")}
            """);

    // ── Cảnh báo sắp xuống hạng ─────────────────────────────────────────────
    public static string CanhBaoXuongHang(
        string tenKhachHang, string tenHang,
        int diemHienTai, int diemCanGiu) =>
        WrapLayout(tenKhachHang, $"⚠️ Hạng {tenHang} của bạn cần được duy trì", $"""
            <p style="color:#334155;line-height:1.6;">
              Điểm tích lũy 12 tháng gần nhất của bạn đang tiến gần ngưỡng tối thiểu
              để duy trì hạng <strong>{tenHang}</strong>.
            </p>
            <table style="width:100%;background:#FFF7ED;border-radius:8px;padding:16px;margin:16px 0;border:1px solid #FED7AA;">
              <tr><td style="color:#64748B;font-size:13px;">Điểm hiện tại (12T)</td>
                  <td align="right" style="font-weight:700;color:{WARNING_COLOR};">{diemHienTai} điểm</td></tr>
              <tr><td style="color:#64748B;font-size:13px;">Cần tối thiểu để giữ hạng</td>
                  <td align="right" style="font-weight:700;color:#0F172A;">{diemCanGiu} điểm</td></tr>
              <tr><td style="color:#64748B;font-size:13px;">Còn thiếu</td>
                  <td align="right" style="font-weight:700;color:{DANGER_COLOR};">{diemCanGiu - diemHienTai} điểm</td></tr>
            </table>
            <p style="color:#64748B;font-size:13px;">
              Hãy thực hiện thêm giao dịch trong thời gian tới để duy trì hạng và quyền lợi hiện tại.
            </p>
            """);

    // ── Helper block voucher dùng chung ─────────────────────────────────────
    private static string VoucherBlock(
        string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, string heading) =>
        maVoucher is null ? "" : $"""
            <div style="background:#F0FDF4;border:1px dashed {SUCCESS_COLOR};border-radius:8px;padding:20px;margin:16px 0;text-align:center;">
              <p style="margin:0 0 4px;font-size:13px;color:#16A34A;">{heading}</p>
              <p style="margin:0 0 12px;font-size:28px;font-weight:700;color:#0F172A;letter-spacing:2px;">{maVoucher}</p>
              <p style="margin:0 0 16px;font-size:13px;color:#64748B;">Giảm {phanTramGiam}% · Hiệu lực 90 ngày</p>
              <a href="{voucherLink}" style="display:inline-block;background:{SUCCESS_COLOR};color:#FFFFFF;text-decoration:none;padding:10px 24px;border-radius:8px;font-weight:600;font-size:14px;">
                Sử dụng ưu đãi ngay
              </a>
            </div>
            <p style="color:#94A3B8;font-size:12px;text-align:center;">
              Sau khi bấm nút, đội ngũ chúng tôi sẽ liên hệ để hỗ trợ bạn áp dụng ưu đãi.
            </p>
            """;
}
