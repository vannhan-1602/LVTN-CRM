using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Services;

// Job này CHỈ làm 1 việc: tự động chuyển hợp đồng đã trễ NgayKetThuc sang "HetHan".
// Việc nhắc gia hạn (còn 60/30/7 ngày) đã tách riêng sang ContractRenewalReminderJobHostedService

public class ContractExpirationJobHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContractExpirationJobHostedService> _logger;

    public ContractExpirationJobHostedService(
        IServiceProvider serviceProvider,
        ILogger<ContractExpirationJobHostedService> logger)
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
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                await ChuyenHetHanAsync(dbContext, today, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContractExpirationJob] Lỗi khi chạy job quét hợp đồng hết hạn.");
            }

            // Quét mỗi ngày 1 lần
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    // Hợp đồng đã trễ NgayKetThuc thật sự -> tự chuyển HetHan.
    private async Task ChuyenHetHanAsync(CrmDbContext dbContext, DateOnly today, CancellationToken ct)
    {
        var expiredContracts = await dbContext.HdHopDongs
            .Where(h => h.TrangThai == "DangThucHien"
                     && h.NgayKetThuc.HasValue
                     && h.NgayKetThuc.Value < today)
            .ToListAsync(ct);

        if (expiredContracts.Count == 0) return;

        foreach (var contract in expiredContracts)
        {
            contract.TrangThai = "HetHan";
            contract.UpdatedAt = DateTime.UtcNow;
        }
        await dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("[ContractExpirationJob] Đã chuyển {Count} hợp đồng sang trạng thái Hết Hạn.", expiredContracts.Count);
    }
}