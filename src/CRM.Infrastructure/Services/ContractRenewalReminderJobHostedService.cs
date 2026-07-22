using CRM.Application.Interfaces.Email;
using CRM.Application.Interfaces.Tickets;
using CRM.Domain.Entities.Tickets;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Chạy 1 lần/ngày, quét HD_HopDong (TrangThai=DangThucHien) còn đúng 60/30/7 ngày nữa
/// là hết hạn (NgayKetThuc) để:
///   - Tạo Ticket loại "Nhắc gia hạn hợp đồng" cho sale phụ trách (lấy từ HD_BaoGia.NhanVien_Id
///     của báo giá gốc trên hợp đồng).
///   - Gửi email nhắc khách hàng (LoaiEmail=NhacGiaHanHopDong).
///
/// Chống nhắc trùng: nếu NgayNhacGiaHanCuoi đã có giá trị trong vòng 7 ngày gần đây thì bỏ qua
/// hợp đồng đó trong lần quét này (đúng yêu cầu nghiệp vụ, không dùng ThoiHan/NgayKetThuc để chặn
/// vì 1 hợp đồng có thể cần nhắc lại ở cả 3 mốc 60/30/7 ngày).
/// </summary>
public class ContractRenewalReminderJobHostedService : BackgroundService
{
    private static readonly int[] MocNhacNgay = { 60, 30, 7 };
    private const string TenLoaiTicketNhacGiaHan = "Nhắc gia hạn hợp đồng";
    private const int SoNgayChongTrung = 7;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContractRenewalReminderJobHostedService> _logger;

    public ContractRenewalReminderJobHostedService(
        IServiceProvider serviceProvider,
        ILogger<ContractRenewalReminderJobHostedService> logger)
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
                _logger.LogError(ex, "[ContractRenewalReminderJob] Lỗi không mong muốn khi quét hợp đồng sắp hết hạn.");
            }

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
        var cacMocNgay = MocNhacNgay.Select(soNgay => today.AddDays(soNgay)).ToHashSet();
        var nguongChongTrung = today.AddDays(-SoNgayChongTrung);

        var hopDongCanNhac = await db.HdHopDongs
            .Where(x => x.TrangThai == "DangThucHien")
            .Where(x => x.NgayKetThuc != null && cacMocNgay.Contains(x.NgayKetThuc.Value))
            .Where(x => x.NgayNhacGiaHanCuoi == null || x.NgayNhacGiaHanCuoi < nguongChongTrung)
            .ToListAsync(ct);

        if (hopDongCanNhac.Count == 0)
        {
            _logger.LogInformation("[ContractRenewalReminderJob] Không có hợp đồng nào cần nhắc gia hạn hôm nay.");
            return;
        }

        var loaiTicketId = await ticketRepository.GetLoaiTicketIdByNameAsync(TenLoaiTicketNhacGiaHan, ct);
        if (loaiTicketId is null)
        {
            _logger.LogWarning(
                "[ContractRenewalReminderJob] Chưa có TK_LoaiTicket '{Ten}'.", TenLoaiTicketNhacGiaHan);
        }

        var khachHangIds = hopDongCanNhac.Select(x => x.KhachHangId).Distinct().ToList();
        var khachHangs = await db.KhKhachHangs
            .Where(k => khachHangIds.Contains(k.Id))
            .ToDictionaryAsync(k => k.Id, ct);

        var baoGiaIds = hopDongCanNhac.Where(x => x.BaoGiaId != null).Select(x => x.BaoGiaId!.Value).Distinct().ToList();
        var baoGias = await db.HdBaoGias
            .Where(b => baoGiaIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, ct);

        int soDaNhac = 0;

        foreach (var hopDong in hopDongCanNhac)
        {
            if (!khachHangs.TryGetValue(hopDong.KhachHangId, out var khachHang)) continue;

            try
            {
                var soNgayConLai = hopDong.NgayKetThuc!.Value.DayNumber - today.DayNumber;

                uint? nhanVienId = hopDong.BaoGiaId is not null && baoGias.TryGetValue(hopDong.BaoGiaId.Value, out var baoGia)
                    ? baoGia.NhanVien_Id
                    : khachHang.NhanVienPhuTrachId;

                var maTicket = await ticketRepository.GenerateMaTicketAsync(ct);
                var ticket = new Ticket
                {
                    MaTicket = maTicket,
                    TieuDe = $"Nhắc gia hạn hợp đồng {hopDong.MaHopDong} (còn {soNgayConLai} ngày)",
                    MoTa = $"Hợp đồng {hopDong.MaHopDong} của khách hàng {khachHang.TenKhachHang} sẽ hết hạn vào " +
                           $"{hopDong.NgayKetThuc:dd/MM/yyyy} (còn {soNgayConLai} ngày). Vui lòng liên hệ khách để tư vấn gia hạn.",
                    LoaiTicketId = loaiTicketId,
                    KhachHangId = khachHang.Id,
                    HopDongId = hopDong.Id,
                    MucDoUuTien = TicketPriority.TrungBinh,
                    NguonTiepNhan = TicketSource.Web,
                    TrangThai = TicketStatus.Moi,
                    NhanVienTiepNhanId = nhanVienId,
                    NgayHenXuLy = DateTime.UtcNow.AddDays(3),
                };

                await ticketRepository.AddAsync(ticket, ct);

                if (!string.IsNullOrWhiteSpace(khachHang.Email))
                {
                    await emailService.GuiEmailNhacGiaHanHopDongAsync(
                        khachHang.Id, khachHang.TenKhachHang, khachHang.Email,
                        hopDong.MaHopDong, hopDong.NgayKetThuc.Value, soNgayConLai, ct);
                }

                hopDong.NgayNhacGiaHanCuoi = today;
                soDaNhac++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[ContractRenewalReminderJob] Lỗi xử lý hợp đồng {MaHopDong}", hopDong.MaHopDong);
            }
        }

        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[ContractRenewalReminderJob] Hoàn tất: đã nhắc gia hạn {SoLuong} hợp đồng.", soDaNhac);
    }
}
