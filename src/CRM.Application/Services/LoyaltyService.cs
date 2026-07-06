using CRM.Application.Common.Models;
using CRM.Application.Features.DanhMuc.DTOs;
using CRM.Application.Interfaces.DanhMuc;
using CRM.Application.Interfaces.Email;
using CRM.Application.Interfaces.Loyalty;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
    private readonly IDanhMucRepository _danhMuc;
    private readonly IEmailService _email;
    private readonly LoyaltyOptions _options;
    private readonly ILogger<LoyaltyService> _logger;

    private string BaseUrl => _options.BaseUrl;

    public LoyaltyService(
        ILoyaltyRepository repo,
        IDanhMucRepository danhMuc,
        IEmailService email,
        IOptions<LoyaltyOptions> options,
        ILogger<LoyaltyService> logger)
    {
        _repo = repo;
        _danhMuc = danhMuc;
        _email = email;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Entry point — gọi ngay sau khi INSERT KT_PhieuThuChi (loại Thu) thành công.
    /// khachHangEmail có thể null nếu khách chưa có email → bỏ qua bước gửi mail.
    /// </summary>
    public async Task XuLySauPhieuThuAsync(
        ulong khachHangId,
        string tenKhachHang,
        string? khachHangEmail,
        string maHoaDon,
        decimal soTienThu,
        ulong hoaDonId,
        ulong phieuThuChiId,
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
        bool daThayDoi;
        ushort? hangCuId;
        int tongDiem12Thang;

        try
        {
            (hangMoiId, daThayDoi, hangCuId) =
                await _repo.TinhLaiHangAsync(khachHangId, ct);

            var (tongDiem, _) = await _repo.GetTichLuy12ThangAsync(khachHangId, ct);
            tongDiem12Thang = tongDiem;

            _logger.LogInformation(
                "[Loyalty] KH {Id}: hạng {Cu} → {Moi}, thay đổi={Doi}",
                khachHangId, hangCuId, hangMoiId, daThayDoi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Loyalty] Lỗi tính hạng KH {Id}", khachHangId);
            tongDiem12Thang = diemVuaCong;
            daThayDoi = false;
            hangMoiId = 0;
            hangCuId = null;
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
        ulong khachHangId,
        string tenKhachHang,
        string email,
        ushort? hangCuId,
        ushort hangMoiId,
        CancellationToken ct)
    {
        bool laThangHang = hangMoiId > (hangCuId ?? 0);

        // Lấy tên hạng cũ và mới, mốc điểm từ DB
        var dsHang = await _repo.GetXepHangInfoAsync(new[] { hangCuId ?? 0, hangMoiId }, ct);
        var hangCuInfo = dsHang.FirstOrDefault(h => h.Id == (hangCuId ?? 0));
        var hangMoiInfo = dsHang.FirstOrDefault(h => h.Id == hangMoiId);

        string tenHangCu = hangCuInfo?.TenHang ?? "Chưa xếp hạng";
        string tenHangMoi = hangMoiInfo?.TenHang ?? $"Hạng #{hangMoiId}";

        if (laThangHang)
        {
            // ── Thăng hạng ────────────────────────────────────────────────
            string? maVoucher = null;
            ulong? voucherId = null;
            decimal? phanTramGiam = null;
            string? voucherLink = null;
            string moTaQuyenLoi = hangMoiInfo?.MoTaQuyenLoi ?? "";

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

                    maVoucher = voucher.MaVoucher;
                    voucherId = voucher.Id;
                    phanTramGiam = voucher.GiaTriGiam;
                    voucherLink = token is not null
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
            int diemCanDat = hangCuInfo?.DiemToiThieu ?? 0;

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

    // ══════════════════════════════════════════════════════════════════════════
    // JOB HÀNG NGÀY — gọi từ LoyaltyDailyJobHostedService (Infrastructure).
    // Gộp 3 việc: (1) sinh nhật/ngày thành lập, (2) ngày lễ, (3) cảnh báo xuống hạng.
    // Đầu mỗi tháng còn tính lại hạng cho toàn bộ khách hàng (điểm rolling-12-tháng có
    // thể tụt dưới ngưỡng dù khách không phát sinh giao dịch mới).
    // ══════════════════════════════════════════════════════════════════════════

    private const int SoNgayGuiTruocSinhNhat = 3;

    public async Task ChayJobHangNgayAsync(CancellationToken ct = default)
    {
        await XuLySinhNhatVaNgayThanhLapAsync(ct);
        await XuLyNgayLeAsync(ct);
        await XuLyCanhBaoXuongHangAsync(ct);

        // Job đầu tháng: tính lại hạng cho toàn bộ KH (chạy trong lần đầu tiên của job
        // trong ngày 1 hàng tháng — hosted service chạy 1 lần/ngày nên không bị lặp).
        if (DateTime.UtcNow.Day == 1)
            await TinhLaiHangChoTatCaAsync(ct);
    }

    private async Task XuLySinhNhatVaNgayThanhLapAsync(CancellationToken ct)
    {
        List<KhachHangNgayDacBiet> danhSach;
        try
        {
            danhSach = await _repo.GetKhachHangNgayDacBietAsync(SoNgayGuiTruocSinhNhat, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Loyalty] Lỗi lấy danh sách sinh nhật/ngày thành lập");
            return;
        }

        foreach (var kh in danhSach.Where(x => x.Email is not null))
        {
            var loaiEmail = kh.LoaiNgay; // "SinhNhat" | "NgayThanhLap"

            // Tránh gửi trùng: GetKhachHangNgayDacBietAsync trả về cùng 1 KH trong suốt cửa sổ
            // N ngày trước ngày đặc biệt, nên phải tự kiểm tra đã gửi trong năm chưa TRƯỚC khi
            // phát voucher (bản thân Gui...Async cũng tự kiểm tra lại trước khi gửi mail).
            if (await _repo.DaGuiEmailTrongNamAsync(kh.KhachHangId, loaiEmail, DateTime.UtcNow.Year, ct))
                continue;

            var (voucherId, maVoucher, phanTramGiam, voucherLink) =
                await TaoVoucherNeuDuHangAsync(kh.KhachHangId, kh.HangHienTaiId, loaiEmail, ct);

            try
            {
                if (loaiEmail == "SinhNhat")
                {
                    await _email.GuiEmailSinhNhatAsync(
                        kh.KhachHangId, kh.TenKhachHang, kh.Email!,
                        voucherId, maVoucher, phanTramGiam, voucherLink, ct);
                }
                else
                {
                    await _email.GuiEmailNgayThanhLapAsync(
                        kh.KhachHangId, kh.TenKhachHang, kh.Email!,
                        voucherId, maVoucher, phanTramGiam, voucherLink, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Loyalty] Lỗi gửi email {Loai} cho KH {Id}", loaiEmail, kh.KhachHangId);
            }
        }
    }

    private async Task XuLyNgayLeAsync(CancellationToken ct)
    {
        List<NgayLeDto> danhSachNgayLe;
        try
        {
            danhSachNgayLe = (await _danhMuc.GetAllNgayLeAsync(ct))
                .Where(x => x.IsActive)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Loyalty] Lỗi lấy danh sách ngày lễ");
            return;
        }

        var today = DateTime.UtcNow.Date;

        foreach (var le in danhSachNgayLe)
        {
            // Tính ngày lễ năm nay (né lỗi 29/2 nếu ngày lễ rơi vào ngày không tồn tại năm nay)
            DateTime ngayLeNamNay;
            try
            {
                ngayLeNamNay = new DateTime(today.Year, le.Thang, le.Ngay);
            }
            catch (ArgumentOutOfRangeException)
            {
                continue;
            }

            var ngayGui = ngayLeNamNay.AddDays(-le.SoNgayGuiTruoc);
            // Ngày lễ đầu năm (vd 1/1) có thể có ngày gửi rơi vào cuối năm trước — thử luôn
            // mốc năm sau để không bỏ sót trường hợp ngayGui < 1/1 năm nay.
            if (ngayGui.Date != today)
            {
                var ngayLeNamSau = ngayLeNamNay.AddYears(1);
                var ngayGuiChoNamSau = ngayLeNamSau.AddDays(-le.SoNgayGuiTruoc);
                if (ngayGuiChoNamSau.Date != today) continue;
            }

            List<KhachHangNgayDacBiet> khachHangs;
            try
            {
                khachHangs = await _repo.GetKhachHangChoNgayLeAsync(le.ApDungChoLoaiKH, le.HangToiThieuApDung, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Loyalty] Lỗi lấy KH cho ngày lễ {Le}", le.TenNgayLe);
                continue;
            }

            var loaiEmail = $"NgayLe_{le.TenNgayLe.Replace(" ", "")}";

            foreach (var kh in khachHangs.Where(x => x.Email is not null))
            {
                if (await _repo.DaGuiEmailTrongNamAsync(kh.KhachHangId, loaiEmail, today.Year, ct))
                    continue;

                var (voucherId, maVoucher, phanTramGiam, voucherLink) =
                    await TaoVoucherNeuDuHangAsync(kh.KhachHangId, kh.HangHienTaiId, loaiEmail, ct);

                try
                {
                    await _email.GuiEmailNgayLeAsync(
                        kh.KhachHangId, kh.TenKhachHang, kh.Email!,
                        le.TenNgayLe, voucherId, maVoucher, phanTramGiam, voucherLink, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Loyalty] Lỗi gửi email ngày lễ {Le} cho KH {Id}", le.TenNgayLe, kh.KhachHangId);
                }
            }
        }
    }

    private async Task XuLyCanhBaoXuongHangAsync(CancellationToken ct)
    {
        List<ulong> ids;
        try
        {
            ids = await _repo.GetAllKhachHangIdsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Loyalty] Lỗi lấy danh sách KH cho cảnh báo xuống hạng");
            return;
        }

        foreach (var id in ids)
        {
            try
            {
                var hangHienTaiId = await _repo.GetHangHienTaiAsync(id, ct);
                if (!hangHienTaiId.HasValue || hangHienTaiId.Value == 0) continue;

                var (tongDiem, _) = await _repo.GetTichLuy12ThangAsync(id, ct);
                var hangInfo = (await _repo.GetXepHangInfoAsync(new[] { hangHienTaiId.Value }, ct))
                    .FirstOrDefault();
                if (hangInfo is null) continue;

                // Chỉ cảnh báo khi điểm đã tụt DƯỚI ngưỡng hạng hiện tại nhưng hạng trong DB
                // chưa được tính lại (sắp bị hạ ở lần tính hạng kế tiếp).
                if (tongDiem >= hangInfo.DiemToiThieu) continue;

                if (await _repo.DaGuiEmailTrongNamAsync(id, "CanhBaoXuongHang", DateTime.UtcNow.Year, ct))
                    continue;

                var thongTin = await _repo.GetTenVaEmailAsync(id, ct);
                if (thongTin is null || string.IsNullOrWhiteSpace(thongTin.Value.Email)) continue;

                await _email.GuiEmailCanhBaoXuongHangAsync(
                    id, thongTin.Value.TenKhachHang, thongTin.Value.Email!,
                    hangInfo.TenHang, tongDiem, hangInfo.DiemToiThieu, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Loyalty] Lỗi kiểm tra cảnh báo xuống hạng KH {Id}", id);
            }
        }
    }

    private async Task TinhLaiHangChoTatCaAsync(CancellationToken ct)
    {
        List<ulong> ids;
        try
        {
            ids = await _repo.GetAllKhachHangIdsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Loyalty] Lỗi lấy danh sách KH cho job tính lại hạng đầu tháng");
            return;
        }

        foreach (var id in ids)
        {
            try
            {
                var (hangMoiId, daThayDoi, hangCuId) = await _repo.TinhLaiHangAsync(id, ct);
                if (!daThayDoi) continue;
                // Email cần tên/email khách — bỏ qua nếu không lấy được (đã log lỗi ở TinhLaiHangAsync nếu có).
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Loyalty] Lỗi tính lại hạng định kỳ KH {Id}", id);
            }
        }
    }

    /// <summary>Phát voucher nếu hạng hiện tại của KH có % giảm > 0. Trả về (VoucherId, MaVoucher, %, Link).</summary>
    private async Task<(ulong? VoucherId, string? MaVoucher, decimal? PhanTramGiam, string? VoucherLink)>
        TaoVoucherNeuDuHangAsync(ulong khachHangId, ushort? hangHienTaiId, string lyDo, CancellationToken ct)
    {
        if (!hangHienTaiId.HasValue || hangHienTaiId.Value == 0) return (null, null, null, null);

        var hangInfo = (await _repo.GetXepHangInfoAsync(new[] { hangHienTaiId.Value }, ct)).FirstOrDefault();
        if (hangInfo is null || hangInfo.PhanTramGiamVoucher <= 0) return (null, null, null, null);

        try
        {
            var voucher = await _repo.PhatVoucherAsync(khachHangId, hangHienTaiId.Value, lyDo, null, ct);
            var token = await _repo.GetLatestTokenByVoucherAsync(voucher.Id, ct);
            var link = token is not null ? $"{BaseUrl}/api/voucher/redeem?token={token}" : null;
            return (voucher.Id, voucher.MaVoucher, voucher.GiaTriGiam, link);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Loyalty] Lỗi phát voucher ({LyDo}) cho KH {Id}", lyDo, khachHangId);
            return (null, null, null, null);
        }
    }
}