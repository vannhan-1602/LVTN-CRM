using CRM.Application.Interfaces.Email;
using CRM.Application.Interfaces.Loyalty;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Services;

/// <summary>
/// Orchestrator cho toàn bộ nghiệp vụ Loyalty.
/// Được gọi từ CreatePhieuThuChiCommandHandler sau khi tạo phiếu thu thành công.
///
/// Flow:
///   1. Cộng điểm (100.000 VNĐ = 1 điểm, idempotent theo PhieuThuChiId)
///   2. Tính lại hạng (rolling 12 tháng)
///   3a. Thăng hạng → phát voucher (nếu hạng có % > 0) → gửi email thăng hạng + link voucher
///   3b. Xuống hạng → gửi email thông báo xuống hạng
///   4. Gửi email xác nhận thanh toán + điểm vừa cộng
///
/// Mọi lỗi email/voucher được catch & log, KHÔNG làm rollback giao dịch phiếu thu.
/// </summary>
public class LoyaltyService
{
    private readonly ILoyaltyRepository _repo;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly ILogger<LoyaltyService> _logger;

    private string BaseUrl => _config["Email:BaseUrl"] ?? "http://localhost:5173";

    public LoyaltyService(
        ILoyaltyRepository repo,
        IEmailService email,
        IConfiguration config,
        ILogger<LoyaltyService> logger)
    {
        _repo   = repo;
        _email  = email;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Entry point — gọi ngay sau khi INSERT KT_PhieuThuChi (loại Thu) thành công.
    /// khachHangEmail có thể null nếu khách chưa có email → bỏ qua bước gửi mail.
    /// </summary>
    public async Task XuLySauPhieuThuAsync(
        ulong   khachHangId,
        string  tenKhachHang,
        string? khachHangEmail,
        string  maHoaDon,
        decimal soTienThu,
        ulong   hoaDonId,
        ulong   phieuThuChiId,
        CancellationToken ct = default)
    {
        // ── Bước 1: Cộng điểm ─────────────────────────────────────────────
        int diemVuaCong;
        try
        {
            diemVuaCong = await _repo.CongDiemAsync(
                khachHangId, soTienThu, hoaDonId, phieuThuChiId, ct);

            _logger.LogInformation(
                "[Loyalty] Cộng {Diem} điểm cho KH {Id} từ phiếu thu {PTC}",
                diemVuaCong, khachHangId, phieuThuChiId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Loyalty] Lỗi cộng điểm KH {Id}", khachHangId);
            return; // Dừng lại nếu cộng điểm lỗi — không tính hạng
        }

        // ── Bước 2: Tính lại hạng ─────────────────────────────────────────
        ushort hangMoiId;
        bool   daThayDoi;
        ushort? hangCuId;
        int    tongDiem12Thang;

        try
        {
            (hangMoiId, daThayDoi, hangCuId) =
                await _repo.TinhLaiHangAsync(khachHangId, ct);

            var (tongDiem, _) = await _repo.GetTichLuy12ThangAsync(khachHangId, ct);
            tongDiem12Thang   = tongDiem;

            _logger.LogInformation(
                "[Loyalty] KH {Id}: hạng {Cu} → {Moi}, thay đổi={Doi}",
                khachHangId, hangCuId, hangMoiId, daThayDoi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Loyalty] Lỗi tính hạng KH {Id}", khachHangId);
            tongDiem12Thang = diemVuaCong;
            daThayDoi       = false;
            hangMoiId       = 0;
            hangCuId        = null;
        }

        // ── Bước 3: Xử lý thay đổi hạng ──────────────────────────────────
        if (daThayDoi && khachHangEmail is not null)
        {
            await XuLyThayDoiHangAsync(
                khachHangId, tenKhachHang, khachHangEmail,
                hangCuId, hangMoiId, ct);
        }

        // ── Bước 4: Email xác nhận thanh toán + điểm ──────────────────────
        if (khachHangEmail is not null && diemVuaCong > 0)
        {
            try
            {
                await _email.GuiEmailXacNhanThanhToanAsync(
                    khachHangId, tenKhachHang, khachHangEmail,
                    maHoaDon, soTienThu, diemVuaCong, tongDiem12Thang, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Loyalty] Lỗi gửi email xác nhận KH {Id}", khachHangId);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════════

    private async Task XuLyThayDoiHangAsync(
        ulong   khachHangId,
        string  tenKhachHang,
        string  email,
        ushort? hangCuId,
        ushort  hangMoiId,
        CancellationToken ct)
    {
        bool laThangHang = hangMoiId > (hangCuId ?? 0);

        // Lấy tên hạng cũ và mới, mốc điểm từ DB
        var dsHang = await _repo.GetXepHangInfoAsync(new[] { hangCuId ?? 0, hangMoiId }, ct);
        var hangCuInfo  = dsHang.FirstOrDefault(h => h.Id == (hangCuId ?? 0));
        var hangMoiInfo = dsHang.FirstOrDefault(h => h.Id == hangMoiId);

        string tenHangCu  = hangCuInfo?.TenHang  ?? "Chưa xếp hạng";
        string tenHangMoi = hangMoiInfo?.TenHang ?? $"Hạng #{hangMoiId}";

        if (laThangHang)
        {
            // ── Thăng hạng ────────────────────────────────────────────────
            string?  maVoucher    = null;
            ulong?   voucherId    = null;
            decimal? phanTramGiam = null;
            string?  voucherLink  = null;
            string   moTaQuyenLoi = hangMoiInfo?.MoTaQuyenLoi ?? "";

            // Phát voucher nếu hạng có % giảm > 0
            if ((hangMoiInfo?.PhanTramGiamVoucher ?? 0) > 0)
            {
                try
                {
                    // Lấy lichSuHangId vừa tạo
                    var lichSuList = await _repo.GetLichSuHangAsync(khachHangId, 1, ct);
                    ulong? lichSuHangId = lichSuList.FirstOrDefault()?.Id;

                    var voucher = await _repo.PhatVoucherAsync(
                        khachHangId, hangMoiId, "ThangHang", lichSuHangId, ct);

                    // Lấy token bảo mật vừa tạo để build link
                    var token = await _repo.GetLatestTokenByVoucherAsync(voucher.Id, ct);

                    maVoucher    = voucher.MaVoucher;
                    voucherId    = voucher.Id;
                    phanTramGiam = voucher.GiaTriGiam;
                    voucherLink  = token is not null
                        ? $"{BaseUrl}/api/voucher/redeem?token={token}"
                        : null;

                    _logger.LogInformation(
                        "[Loyalty] Phát voucher {Ma} ({Pct}%) cho KH {Id} thăng lên {Hang}",
                        maVoucher, phanTramGiam, khachHangId, tenHangMoi);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Loyalty] Lỗi phát voucher KH {Id}", khachHangId);
                }
            }

            try
            {
                await _email.GuiEmailThangHangAsync(
                    khachHangId, tenKhachHang, email,
                    tenHangCu, tenHangMoi, moTaQuyenLoi,
                    voucherId, maVoucher, phanTramGiam, voucherLink, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Loyalty] Lỗi gửi email thăng hạng KH {Id}", khachHangId);
            }
        }
        else
        {
            // ── Xuống hạng ────────────────────────────────────────────────
            var (tongDiem, _) = await _repo.GetTichLuy12ThangAsync(khachHangId, ct);
            int diemCanDat    = hangCuInfo?.DiemToiThieu ?? 0;

            try
            {
                await _email.GuiEmailXuongHangAsync(
                    khachHangId, tenKhachHang, email,
                    tenHangCu, tenHangMoi,
                    tongDiem, diemCanDat, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Loyalty] Lỗi gửi email xuống hạng KH {Id}", khachHangId);
            }
        }
    }
}
