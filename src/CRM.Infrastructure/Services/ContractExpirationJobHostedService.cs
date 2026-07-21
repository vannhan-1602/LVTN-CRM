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

                var expiredContracts = await dbContext.HdHopDongs
                    .Where(h => h.TrangThai == "DangThucHien"
                             && h.NgayKetThuc.HasValue
                             && h.NgayKetThuc.Value < today)
                    .ToListAsync(stoppingToken);

                if (expiredContracts.Any())
                {
                    foreach (var contract in expiredContracts)
                    {
                        contract.TrangThai = "HetHan";
                        contract.UpdatedAt = DateTime.UtcNow;
                    }
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Đã chuyển {Count} hợp đồng sang trạng thái Hết Hạn.", expiredContracts.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi quét cập nhật trạng thái hợp đồng hết hạn.");
            }

            // Quét mỗi ngày 1 lần
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}