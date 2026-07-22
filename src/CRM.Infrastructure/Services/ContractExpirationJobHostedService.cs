using CRM.Application.Interfaces.Email;
using CRM.Application.Interfaces.Tickets;
using CRM.Domain.Entities.Tickets;
using CRM.Domain.Enums;
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
    private static readonly int[] MocCanhBaoNgay = { 60, 30, 7 };
    private const string TenLoaiTicketNhacGiaHan = "Nhắc gia hạn hợp đồng";

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
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                await NhacGiaHanSapHetHanAsync(dbContext, emailService, ticketRepository, today, stoppingToken);
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

    // Hợp đồng còn đúng 60/30/7 ngày -> gửi email khách + tạo Ticket nhắc nhân viên phụ trách gia hạn.
    private async Task NhacGiaHanSapHetHanAsync(
        CrmDbContext dbContext, IEmailService emailService, ITicketRepository ticketRepository,
        DateOnly today, CancellationToken ct)
    {
        var mocNgay = MocCanhBaoNgay.Select(soNgay => today.AddDays(soNgay)).ToHashSet();

        var sapHetHan = await (
            from hd in dbContext.HdHopDongs
            where hd.TrangThai == "DangThucHien" && hd.NgayKetThuc.HasValue && mocNgay.Contains(hd.NgayKetThuc.Value)
            join kh in dbContext.KhKhachHangs on hd.KhachHangId equals kh.Id
            select new
            {
                hd.Id,
                hd.MaHopDong,
                hd.NgayKetThuc,
                KhachHangId = kh.Id,
                kh.TenKhachHang,
                kh.Email,
                kh.NhanVienPhuTrachId
            }
        ).ToListAsync(ct);

        if (sapHetHan.Count == 0) return;

        var loaiTicketId = await ticketRepository.GetLoaiTicketIdByNameAsync(TenLoaiTicketNhacGiaHan, ct);
        if (loaiTicketId is null)
        {
            _logger.LogWarning(
                "[ContractExpirationJob] Chưa có TK_LoaiTicket '{Ten}' — vẫn gửi email nhưng ticket sẽ không gán loại.",
                TenLoaiTicketNhacGiaHan);
        }

        foreach (var hd in sapHetHan)
        {
            var soNgayConLai = hd.NgayKetThuc!.Value.DayNumber - today.DayNumber;

            if (!string.IsNullOrWhiteSpace(hd.Email))
            {
                try
                {
                    await emailService.GuiEmailNhacGiaHanHopDongAsync(
                        hd.KhachHangId, hd.TenKhachHang, hd.Email!,
                        hd.MaHopDong, hd.NgayKetThuc.Value, soNgayConLai, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[ContractExpirationJob] Gửi email nhắc gia hạn hợp đồng {Ma} thất bại.", hd.MaHopDong);
                }
            }

            try
            {
                var maTicket = await ticketRepository.GenerateMaTicketAsync(ct);
                var uuTien = soNgayConLai <= 7 ? TicketPriority.Cao : TicketPriority.TrungBinh;

                var ticket = new Ticket
                {
                    MaTicket = maTicket,
                    TieuDe = $"Hợp đồng {hd.MaHopDong} sắp hết hạn ({soNgayConLai} ngày)",
                    MoTa = $"Hợp đồng {hd.MaHopDong} sẽ hết hạn vào {hd.NgayKetThuc.Value:dd/MM/yyyy} " +
                           $"(còn {soNgayConLai} ngày). Vui lòng liên hệ khách hàng để trao đổi gia hạn.",
                    LoaiTicketId = loaiTicketId,
                    KhachHangId = hd.KhachHangId,
                    HopDongId = hd.Id,
                    MucDoUuTien = uuTien,
                    NguonTiepNhan = TicketSource.Web,
                    TrangThai = TicketStatus.Moi,
                    NhanVienTiepNhanId = hd.NhanVienPhuTrachId,
                    NgayHenXuLy = DateTime.UtcNow.AddDays(1),
                };

                await ticketRepository.AddAsync(ticket, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContractExpirationJob] Tạo ticket nhắc gia hạn cho hợp đồng {Ma} thất bại.", hd.MaHopDong);
            }
        }

        _logger.LogInformation("[ContractExpirationJob] Đã nhắc gia hạn {Count} hợp đồng sắp hết hạn.", sapHetHan.Count);
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