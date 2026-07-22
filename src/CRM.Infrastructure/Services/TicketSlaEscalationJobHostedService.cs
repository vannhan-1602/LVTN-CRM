using CRM.Application.Interfaces.Email;
using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Chạy mỗi giờ, quét TK_Ticket còn mở (TrangThai != Dong, chưa xóa) mà đã quá ThoiHanSLA:
///   - Tăng SoLanEscalate, gửi email cảnh báo cho nhân viên xử lý (NhanVienXuLy_Id).
///   - Nếu SoLanEscalate >= 2 (đã cảnh báo từ 2 lần trở lên) thì cảnh báo thêm cho Quản lý (role Manager).
///
/// Idempotent theo giờ: mỗi lần quét chỉ gửi 1 email/ticket (tăng SoLanEscalate ngay khi gửi),
/// nên không gửi trùng nhiều lần trong cùng 1 giờ chạy — nhưng ticket vẫn còn quá hạn ở lần quét
/// kế tiếp sẽ tiếp tục được cảnh báo cho tới khi được xử lý xong (đúng ý nghĩa "escalate").
/// </summary>
public class TicketSlaEscalationJobHostedService : BackgroundService
{
    private const uint SoLanEscalateBaoQuanLy = 2;
    private const string TenRoleQuanLy = "Manager";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TicketSlaEscalationJobHostedService> _logger;

    public TicketSlaEscalationJobHostedService(
        IServiceProvider serviceProvider,
        ILogger<TicketSlaEscalationJobHostedService> logger)
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
                _logger.LogError(ex, "[TicketSlaEscalationJob] Lỗi không mong muốn khi quét ticket quá hạn SLA.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ChayJobAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTime.UtcNow;

        var ticketQuaHan = await db.TkTickets
            .Where(x => !x.IsDeleted && x.TrangThai != "Dong")
            .Where(x => x.ThoiHanSLA != null && x.ThoiHanSLA < now)
            .ToListAsync(ct);

        if (ticketQuaHan.Count == 0)
        {
            _logger.LogInformation("[TicketSlaEscalationJob] Không có ticket nào quá hạn SLA.");
            return;
        }

        var nhanVienIds = ticketQuaHan
            .Where(x => x.NhanVienXuLy_Id != null)
            .Select(x => x.NhanVienXuLy_Id!.Value)
            .Distinct()
            .ToList();

        var nhanViens = await db.HtUsers
            .Include(u => u.NhanSu)
            .Where(u => nhanVienIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        // Danh sách email quản lý — gửi thêm khi 1 ticket đã bị escalate từ 2 lần trở lên.
        var quanLyEmails = await db.HtUsers
            .Include(u => u.Role)
            .Include(u => u.NhanSu)
            .Where(u => u.Role != null && u.Role.TenRole == TenRoleQuanLy && u.TrangThai == "Active")
            .Where(u => u.NhanSu != null && u.NhanSu.Email != null)
            .Select(u => new { Ten = u.NhanSu!.HoTen, Email = u.NhanSu!.Email! })
            .ToListAsync(ct);

        int soDaCanhBao = 0;

        foreach (var ticket in ticketQuaHan)
        {
            try
            {
                ticket.SoLanEscalate += 1;

                if (ticket.NhanVienXuLy_Id is not null &&
                    nhanViens.TryGetValue(ticket.NhanVienXuLy_Id.Value, out var nhanVien) &&
                    !string.IsNullOrWhiteSpace(nhanVien.NhanSu?.Email))
                {
                    await emailService.GuiEmailCanhBaoSlaAsync(
                        nhanVien.NhanSu.HoTen, nhanVien.NhanSu.Email,
                        ticket.MaTicket, ticket.TieuDe, ticket.ThoiHanSLA!.Value, ticket.SoLanEscalate, ct);
                }

                if (ticket.SoLanEscalate >= SoLanEscalateBaoQuanLy)
                {
                    foreach (var quanLy in quanLyEmails)
                    {
                        await emailService.GuiEmailCanhBaoSlaAsync(
                            quanLy.Ten, quanLy.Email,
                            ticket.MaTicket, ticket.TieuDe, ticket.ThoiHanSLA!.Value, ticket.SoLanEscalate, ct);
                    }
                }

                soDaCanhBao++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TicketSlaEscalationJob] Lỗi xử lý ticket {MaTicket}", ticket.MaTicket);
            }
        }

        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[TicketSlaEscalationJob] Hoàn tất: đã cảnh báo {SoLuong} ticket quá hạn SLA.", soDaCanhBao);
    }
}
