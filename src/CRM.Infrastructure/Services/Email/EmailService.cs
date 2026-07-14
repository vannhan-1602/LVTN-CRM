using CRM.Application.Interfaces.Email;
using CRM.Application.Interfaces.Loyalty;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CRM.Infrastructure.Services.Email;

/// <summary>
/// Gửi email thật qua Gmail SMTP + MailKit.
/// Sau mỗi lần gửi (thành công hay thất bại) đều ghi log vào KH_EmailLog.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILoyaltyRepository _loyaltyRepo;
    private readonly ILogger<EmailService> _logger;

    private string SmtpHost     => _config["Email:SmtpHost"]     ?? "smtp.gmail.com";
    private int    SmtpPort     => int.Parse(_config["Email:SmtpPort"] ?? "587");
    private string SmtpUser     => _config["Email:SmtpUser"]     ?? "";
    private string SmtpPassword => _config["Email:SmtpPassword"] ?? "";
    private string FromName     => _config["Email:FromName"]     ?? "CRM System";
    private string BaseUrl      => _config["Email:BaseUrl"]      ?? "https://localhost:7001";

    public EmailService(
        IConfiguration config,
        ILoyaltyRepository loyaltyRepo,
        ILogger<EmailService> logger)
    {
        _config      = config;
        _loyaltyRepo = loyaltyRepo;
        _logger      = logger;
    }

    // ── XÁC NHẬN THANH TOÁN + ĐIỂM ─────────────────────────────────────────
    public async Task GuiEmailXacNhanThanhToanAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string maHoaDon, decimal soTienThu, int diemVuaCong, int tongDiem12Thang,
        CancellationToken ct = default)
    {
        var tieuDe = $"[CRM] Xác nhận thanh toán hóa đơn {maHoaDon}";
        var html   = EmailTemplateHelper.XacNhanThanhToan(
            tenKhachHang, maHoaDon, soTienThu, diemVuaCong, tongDiem12Thang);

        await GuiAsync(khachHangId, email, tieuDe, html, "XacNhanThanhToan", null, ct);
    }

    // ── THĂNG HẠNG + VOUCHER ────────────────────────────────────────────────
    public async Task GuiEmailThangHangAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenHangCu, string tenHangMoi, string moTaQuyenLoi,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default)
    {
        var tieuDe = $"[CRM] 🎉 Chúc mừng! Bạn đã lên hạng {tenHangMoi}";
        var html   = EmailTemplateHelper.ThangHang(
            tenKhachHang, tenHangCu, tenHangMoi, moTaQuyenLoi,
            maVoucher, phanTramGiam, voucherLink);

        await GuiAsync(khachHangId, email, tieuDe, html, "ThangHang", voucherId, ct);
    }

    // ── XUỐNG HẠNG ──────────────────────────────────────────────────────────
    public async Task GuiEmailXuongHangAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenHangCu, string tenHangMoi,
        int diemHienTai, int diemCanDat,
        CancellationToken ct = default)
    {
        var tieuDe = $"[CRM] Thông báo thay đổi hạng khách hàng";
        var html   = EmailTemplateHelper.XuongHang(
            tenKhachHang, tenHangCu, tenHangMoi, diemHienTai, diemCanDat);

        await GuiAsync(khachHangId, email, tieuDe, html, "XuongHang", null, ct);
    }

    // ── SINH NHẬT ───────────────────────────────────────────────────────────
    public async Task GuiEmailSinhNhatAsync(
        ulong khachHangId, string tenKhachHang, string email,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default)
    {
        // Chống gửi trùng sinh nhật trong cùng năm
        if (await _loyaltyRepo.DaGuiEmailTrongNamAsync(khachHangId, "SinhNhat", DateTime.UtcNow.Year, ct))
        {
            _logger.LogInformation("Đã gửi email sinh nhật cho KH {Id} năm {Year}, bỏ qua.", khachHangId, DateTime.UtcNow.Year);
            return;
        }

        var tieuDe = "[CRM] 🎂 Chúc mừng sinh nhật!";
        var html   = EmailTemplateHelper.SinhNhat(tenKhachHang, maVoucher, phanTramGiam, voucherLink);

        await GuiAsync(khachHangId, email, tieuDe, html, "SinhNhat", voucherId, ct);
    }

    // ── NGÀY THÀNH LẬP ──────────────────────────────────────────────────────
    public async Task GuiEmailNgayThanhLapAsync(
        ulong khachHangId, string tenKhachHang, string email,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default)
    {
        if (await _loyaltyRepo.DaGuiEmailTrongNamAsync(khachHangId, "NgayThanhLap", DateTime.UtcNow.Year, ct))
        {
            _logger.LogInformation("Đã gửi email ngày thành lập cho KH {Id} năm {Year}, bỏ qua.", khachHangId, DateTime.UtcNow.Year);
            return;
        }

        var tieuDe = "[CRM] 🏢 Chúc mừng kỷ niệm ngày thành lập!";
        var html   = EmailTemplateHelper.NgayThanhLap(tenKhachHang, maVoucher, phanTramGiam, voucherLink);

        await GuiAsync(khachHangId, email, tieuDe, html, "NgayThanhLap", voucherId, ct);
    }

    // ── NGÀY LỄ ─────────────────────────────────────────────────────────────
    public async Task GuiEmailNgayLeAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenNgayLe,
        ulong? voucherId, string? maVoucher, decimal? phanTramGiam,
        string? voucherLink, CancellationToken ct = default)
    {
        var loaiEmail = $"NgayLe_{tenNgayLe.Replace(" ", "")}";
        if (await _loyaltyRepo.DaGuiEmailTrongNamAsync(khachHangId, loaiEmail, DateTime.UtcNow.Year, ct))
        {
            _logger.LogInformation("Đã gửi email ngày lễ '{Le}' cho KH {Id} năm {Year}, bỏ qua.", tenNgayLe, khachHangId, DateTime.UtcNow.Year);
            return;
        }

        var tieuDe = $"[CRM] 🎉 Ưu đãi {tenNgayLe} dành riêng cho bạn";
        var html   = EmailTemplateHelper.NgayLe(tenKhachHang, tenNgayLe, maVoucher, phanTramGiam, voucherLink);

        await GuiAsync(khachHangId, email, tieuDe, html, loaiEmail, voucherId, ct);
    }

    // ── CẢNH BÁO SẮP XUỐNG HẠNG ────────────────────────────────────────────
    public async Task GuiEmailCanhBaoXuongHangAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string tenHangHienTai, int diemHienTai, int diemCanGiu,
        CancellationToken ct = default)
    {
        var tieuDe = $"[CRM] ⚠️ Hạng {tenHangHienTai} của bạn cần được duy trì";
        var html   = EmailTemplateHelper.CanhBaoXuongHang(
            tenKhachHang, tenHangHienTai, diemHienTai, diemCanGiu);

        await GuiAsync(khachHangId, email, tieuDe, html, "CanhBaoXuongHang", null, ct);
    }

    // ── BÁO GIÁ (kèm link công khai xem/chấp nhận/từ chối) ──────────────────
    public async Task GuiEmailBaoGiaAsync(
        ulong khachHangId, string tenKhachHang, string email,
        string maBaoGia, decimal tongTien, string quoteLink,
        CancellationToken ct = default)
    {
        var tieuDe = $"[CRM] 📄 Báo giá {maBaoGia} từ chúng tôi";
        var html   = EmailTemplateHelper.BaoGia(tenKhachHang, maBaoGia, tongTien, quoteLink);

        await GuiAsync(khachHangId, email, tieuDe, html, "BaoGia", null, ct);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // CORE — Gửi email thật qua MailKit + ghi log
    // ══════════════════════════════════════════════════════════════════════════
    private async Task GuiAsync(
        ulong khachHangId, string emailDen, string tieuDe, string htmlBody,
        string loaiEmail, ulong? voucherId,
        CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(FromName, SmtpUser));
        message.To.Add(MailboxAddress.Parse(emailDen));
        message.Subject = tieuDe;
        message.Body    = new TextPart("html") { Text = htmlBody };

        bool thanhCong  = false;
        string? loiChiTiet = null;

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(SmtpUser, SmtpPassword, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            thanhCong = true;
            _logger.LogInformation("✉️  Đã gửi email [{Loai}] tới {Email} (KH {Id})", loaiEmail, emailDen, khachHangId);
        }
        catch (Exception ex)
        {
            loiChiTiet = ex.Message;
            _logger.LogError(ex, "❌ Gửi email [{Loai}] tới {Email} thất bại", loaiEmail, emailDen);
        }
        finally
        {
            // Luôn ghi log dù thành công hay thất bại
            try
            {
                await _loyaltyRepo.GhiEmailLogAsync(
                    khachHangId, loaiEmail, emailDen, tieuDe,
                    thanhCong, voucherId, loiChiTiet, ct);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Ghi EmailLog thất bại");
            }
        }
    }
}
