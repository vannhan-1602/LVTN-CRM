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

/// <summary>
/// Chạy 1 lần/ngày, quét bảng HD_LichThanhToan (lịch trả góp) để:
///   - Đợt sắp đến hạn (còn ≤ SoNgayCanhBaoTruoc ngày, đang ChuaDenHan)
///     → chuyển ChoThanhToan, gửi email nhắc khách + tạo Ticket cho nhân viên phụ trách.
///   - Đợt đã quá hạn (HanThanhToan < hôm nay, chưa QuaHan)
///     → chuyển QuaHan, gửi email cảnh báo khách + tạo Ticket ưu tiên Cao cho nhân viên phụ trách.
///
/// Idempotent theo thiết kế: mỗi đợt chỉ đổi trạng thái 1 lần cho mỗi mốc
/// (ChuaDenHan → ChoThanhToan → QuaHan), nên email/ticket không bị gửi/tạo lặp lại
/// mỗi ngày chạy job — chỉ đúng lúc đợt đó BĂNG QUA ngưỡng lần đầu.
///
/// Không xử lý hóa đơn thanh toán 1 lần (KT_HoaDon) — bảng đó hiện chưa có cột
/// hạn thanh toán riêng, chỉ hợp đồng trả góp (HD_LichThanhToan) mới có lịch cụ thể.
/// </summary>
public class PaymentReminderJobHostedService : BackgroundService
{
    private const int SoNgayCanhBaoTruoc = 3;
    private const string TenLoaiTicketNhacThanhToan = "Nhắc thanh toán";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentReminderJobHostedService> _logger;

    public PaymentReminderJobHostedService(
        IServiceProvider serviceProvider,
        ILogger<PaymentReminderJobHostedService> logger)
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
                _logger.LogError(ex, "[PaymentReminderJob] Lỗi không mong muốn khi quét lịch thanh toán.");
            }

            // Quét mỗi ngày 1 lần
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task ChayJobAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var nguongCanhBao = today.AddDays(SoNgayCanhBaoTruoc);

        // Chỉ lấy các đợt còn "sống" (chưa DaThanhToan) — DaThanhToan là trạng thái cuối,
        // không cần quét lại.
        var dotCanXuLy = await db.HdLichThanhToans
            .Include(x => x.HopDong)
            .Where(x => x.TrangThai == "ChuaDenHan" || x.TrangThai == "ChoThanhToan")
            .Where(x => x.HanThanhToan <= nguongCanhBao)
            .ToListAsync(ct);

        if (dotCanXuLy.Count == 0)
        {
            _logger.LogInformation("[PaymentReminderJob] Không có đợt thanh toán nào cần xử lý hôm nay.");
            return;
        }

        var loaiTicketId = await ticketRepository.GetLoaiTicketIdByNameAsync(TenLoaiTicketNhacThanhToan, ct);
        if (loaiTicketId is null)
        {
            _logger.LogWarning(
                "[PaymentReminderJob] Chưa có TK_LoaiTicket '{Ten}' — cần chạy migration trước khi job này hoạt động đầy đủ.",
                TenLoaiTicketNhacThanhToan);
        }

        // Gom Id khách hàng liên quan để 1 lần truy vấn duy nhất (tránh N+1).
        var khachHangIds = dotCanXuLy
            .Where(x => x.HopDong is not null)
            .Select(x => x.HopDong!.KhachHangId)
            .Distinct()
            .ToList();

        var khachHangs = await db.KhKhachHangs
            .Where(k => khachHangIds.Contains(k.Id))
            .ToDictionaryAsync(k => k.Id, ct);

        int soDaCanhBao = 0, soDaQuaHan = 0;

        foreach (var dot in dotCanXuLy)
        {
            if (dot.HopDong is null) continue;
            if (!khachHangs.TryGetValue(dot.HopDong.KhachHangId, out var khachHang)) continue;

            var quaHan = dot.HanThanhToan < today;

            try
            {
                if (quaHan && dot.TrangThai != "QuaHan")
                {
                    dot.TrangThai = "QuaHan";

                    if (!string.IsNullOrWhiteSpace(khachHang.Email))
                    {
                        await emailService.GuiEmailQuaHanThanhToanAsync(
                            khachHang.Id, khachHang.TenKhachHang, khachHang.Email,
                            dot.HopDong.MaHopDong, dot.SoDot, dot.SoTien, dot.HanThanhToan, ct);
                    }

                    await TaoTicketNhacNhanVienAsync(
                        ticketRepository, loaiTicketId, khachHang.Id, khachHang.NhanVienPhuTrachId,
                        dot.HopDong.Id, dot.HopDong.MaHopDong, dot.SoDot, dot.SoTien, dot.HanThanhToan,
                        quaHan: true, ct);

                    soDaQuaHan++;
                }
                else if (!quaHan && dot.TrangThai == "ChuaDenHan")
                {
                    dot.TrangThai = "ChoThanhToan";

                    if (!string.IsNullOrWhiteSpace(khachHang.Email))
                    {
                        await emailService.GuiEmailNhacThanhToanAsync(
                            khachHang.Id, khachHang.TenKhachHang, khachHang.Email,
                            dot.HopDong.MaHopDong, dot.SoDot, dot.SoTien, dot.HanThanhToan, ct);
                    }

                    await TaoTicketNhacNhanVienAsync(
                        ticketRepository, loaiTicketId, khachHang.Id, khachHang.NhanVienPhuTrachId,
                        dot.HopDong.Id, dot.HopDong.MaHopDong, dot.SoDot, dot.SoTien, dot.HanThanhToan,
                        quaHan: false, ct);

                    soDaCanhBao++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[PaymentReminderJob] Lỗi xử lý đợt {SoDot} hợp đồng {MaHopDong}",
                    dot.SoDot, dot.HopDong.MaHopDong);
            }
        }

        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[PaymentReminderJob] Hoàn tất: {CanhBao} đợt vừa nhắc sắp đến hạn, {QuaHan} đợt vừa chuyển quá hạn.",
            soDaCanhBao, soDaQuaHan);
    }

    private static async Task TaoTicketNhacNhanVienAsync(
        ITicketRepository ticketRepository, ushort? loaiTicketId,
        ulong khachHangId, uint? nhanVienPhuTrachId,
        ulong hopDongId, string maHopDong, int soDot, decimal soTien, DateOnly hanThanhToan,
        bool quaHan, CancellationToken ct)
    {
        var maTicket = await ticketRepository.GenerateMaTicketAsync(ct);

        var tieuDe = quaHan
            ? $"Đợt {soDot} hợp đồng {maHopDong} đã quá hạn thanh toán"
            : $"Nhắc thanh toán đợt {soDot} hợp đồng {maHopDong} sắp đến hạn";

        var moTa = quaHan
            ? $"Đợt {soDot} ({soTien:N0}đ) của hợp đồng {maHopDong} đã quá hạn thanh toán " +
              $"({hanThanhToan:dd/MM/yyyy}). Vui lòng liên hệ khách hàng gấp để thu tiền."
            : $"Đợt {soDot} ({soTien:N0}đ) của hợp đồng {maHopDong} sắp đến hạn thanh toán " +
              $"({hanThanhToan:dd/MM/yyyy}). Vui lòng chủ động liên hệ khách hàng để nhắc thu tiền.";

        var ticket = new Ticket
        {
            MaTicket = maTicket,
            TieuDe = tieuDe,
            MoTa = moTa,
            LoaiTicketId = loaiTicketId,
            KhachHangId = khachHangId,
            HopDongId = hopDongId,
            MucDoUuTien = quaHan ? TicketPriority.Cao : TicketPriority.TrungBinh,
            NguonTiepNhan = TicketSource.Web,
            TrangThai = TicketStatus.Moi,
            NhanVienTiepNhanId = nhanVienPhuTrachId,
            NgayHenXuLy = DateTime.UtcNow.AddDays(1),
        };

        await ticketRepository.AddAsync(ticket, ct);
    }
}