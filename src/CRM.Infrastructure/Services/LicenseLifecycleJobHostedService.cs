using CRM.Application.Interfaces.Email;
using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Chạy 1 lần/ngày, xử lý vòng đời License (HD_License):
///   1) Tự động chuyển các License đã quá NgayHetHan (còn DangHoatDong/TamKhoa) sang HetHan
///      — giống hệt cách ContractExpirationJobHostedService làm cho hợp đồng.
///   2) Gửi email nhắc khách hàng khi License còn ĐÚNG 30 hoặc 7 ngày nữa hết hạn.
///
/// Chống nhắc trùng: KHÔNG dùng cột "đã nhắc lần cuối" như hợp đồng (HD_License không có cột
/// này và không được phép đổi DB) — thay vào đó chỉ nhắc khi NgayHetHan trùng CHÍNH XÁC ngày
/// hôm nay+30 hoặc hôm nay+7, nên mỗi License chỉ khớp điều kiện đúng 1 ngày duy nhất trong cả
/// quá trình — job chạy mỗi ngày sẽ không gửi lặp lại cho cùng 1 mốc.
/// </summary>
public class LicenseLifecycleJobHostedService : BackgroundService
{
    private static readonly int[] MocNhacNgay = { 30, 7 };

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LicenseLifecycleJobHostedService> _logger;

    public LicenseLifecycleJobHostedService(
        IServiceProvider serviceProvider, ILogger<LicenseLifecycleJobHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ChayJobAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LicenseLifecycleJob] Lỗi không mong muốn khi xử lý License.");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task ChayJobAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        await ChuyenHetHanAsync(db, ct);
        await NhacGiaHanAsync(db, emailService, ct);
    }

    // ── 1) Tự động chuyển HetHan ──────────────────────────────────────────────
    private async Task ChuyenHetHanAsync(CrmDbContext db, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var expired = await db.HdLicenses
            .Where(x => x.TrangThai != "HetHan" && x.NgayHetHan != null && x.NgayHetHan.Value < today)
            .ToListAsync(ct);

        if (expired.Count == 0) return;

        foreach (var license in expired)
        {
            license.TrangThai = "HetHan";
            license.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);

        _logger.LogInformation("[LicenseLifecycleJob] Đã chuyển {SoLuong} license sang HetHan.", expired.Count);
    }

    // ── 2) Nhắc gia hạn qua email ─────────────────────────────────────────────
    private async Task NhacGiaHanAsync(CrmDbContext db, IEmailService emailService, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cacMocNgay = MocNhacNgay.Select(soNgay => today.AddDays(soNgay)).ToHashSet();

        var licenseCanNhac = await db.HdLicenses
            .Where(x => x.TrangThai == "DangHoatDong")
            .Where(x => x.NgayHetHan != null && cacMocNgay.Contains(x.NgayHetHan.Value))
            .Include(x => x.SanPham)
            .Include(x => x.HopDong)
            .ToListAsync(ct);

        if (licenseCanNhac.Count == 0)
        {
            _logger.LogInformation("[LicenseLifecycleJob] Không có license nào cần nhắc gia hạn hôm nay.");
            return;
        }

        var khachHangIds = licenseCanNhac
            .Where(x => x.HopDong != null)
            .Select(x => x.HopDong!.KhachHangId)
            .Distinct()
            .ToList();
        var khachHangs = await db.KhKhachHangs
            .Where(k => khachHangIds.Contains(k.Id))
            .ToDictionaryAsync(k => k.Id, ct);

        int soDaNhac = 0;

        foreach (var license in licenseCanNhac)
        {
            if (license.HopDong is null) continue;
            if (!khachHangs.TryGetValue(license.HopDong.KhachHangId, out var khachHang)) continue;
            if (string.IsNullOrWhiteSpace(khachHang.Email)) continue;

            try
            {
                var soNgayConLai = license.NgayHetHan!.Value.DayNumber - today.DayNumber;

                await emailService.GuiEmailNhacGiaHanLicenseAsync(
                    khachHang.Id, khachHang.TenKhachHang, khachHang.Email,
                    license.MaLicenseKey ?? $"#{license.Id}",
                    license.SanPham?.TenSP ?? "(không rõ sản phẩm)",
                    license.NgayHetHan.Value, soNgayConLai, ct);

                soDaNhac++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[LicenseLifecycleJob] Lỗi gửi email nhắc gia hạn License {MaLicenseKey}", license.MaLicenseKey);
            }
        }

        _logger.LogInformation("[LicenseLifecycleJob] Hoàn tất: đã nhắc gia hạn {SoLuong} license.", soDaNhac);
    }
}
