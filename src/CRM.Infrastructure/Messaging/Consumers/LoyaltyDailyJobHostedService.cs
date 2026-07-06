using CRM.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Messaging.Consumers;

/// <summary>
/// Chạy 1 lần/ngày, gọi LoyaltyService.ChayJobHangNgayAsync để xử lý:
/// email sinh nhật/ngày thành lập, email ngày lễ, cảnh báo xuống hạng,
/// và (vào ngày 1 hàng tháng) tính lại hạng cho toàn bộ khách hàng.
///
/// LoyaltyService là Scoped nên hosted service phải tự tạo scope mỗi lần chạy
/// (không thể inject trực tiếp vào constructor của 1 Singleton BackgroundService).
/// </summary>
public class LoyaltyDailyJobHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LoyaltyDailyJobHostedService> _logger;

    // Giờ chạy job trong ngày (UTC). Có thể cấu hình lại nếu cần chạy theo giờ VN.
    private static readonly TimeSpan GioChayTrongNgay = TimeSpan.FromHours(1);

    public LoyaltyDailyJobHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<LoyaltyDailyJobHostedService> logger)
    {
        _scopeFactory = scopeFactory;
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
                _logger.LogError(ex, "[LoyaltyDailyJob] Lỗi không mong muốn khi chạy job hàng ngày");
            }

            var delay = TinhThoiGianChoLanChayKeTiep();
            _logger.LogInformation("[LoyaltyDailyJob] Lần chạy kế tiếp sau {Delay}", delay);
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task ChayJobAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var loyaltyService = scope.ServiceProvider.GetRequiredService<LoyaltyService>();

        _logger.LogInformation("[LoyaltyDailyJob] Bắt đầu chạy job hàng ngày lúc {Now}", DateTime.UtcNow);
        await loyaltyService.ChayJobHangNgayAsync(ct);
        _logger.LogInformation("[LoyaltyDailyJob] Hoàn tất job hàng ngày lúc {Now}", DateTime.UtcNow);
    }

    private static TimeSpan TinhThoiGianChoLanChayKeTiep()
    {
        var now = DateTime.UtcNow;
        var todayRun = now.Date + GioChayTrongNgay;
        var nextRun = now < todayRun ? todayRun : todayRun.AddDays(1);
        return nextRun - now;
    }
}
