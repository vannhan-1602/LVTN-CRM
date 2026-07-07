using CRM.Application.Features.Analytics.DTOs;
using CRM.Application.Interfaces.Analytics;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly CrmDbContext _context;
    public AnalyticsRepository(CrmDbContext context) => _context = context;

    public async Task<SalesAnalyticsDataDto> GetSalesAnalyticsDataAsync(int soThang, CancellationToken ct = default)
    {
        var tuNgay = DateTime.UtcNow.AddMonths(-Math.Max(1, soThang)).Date;

        // ── Doanh thu theo tháng (dựa trên hóa đơn tạo trong khoảng thời gian) ──
        var doanhThuTheoThang = await _context.KtHoaDons
            .AsNoTracking()
            .Where(h => h.CreatedAt != null && h.CreatedAt >= tuNgay)
            .GroupBy(h => new { h.CreatedAt!.Value.Year, h.CreatedAt!.Value.Month })
            .Select(g => new DoanhThuThangDto
            {
                Nam = g.Key.Year,
                Thang = g.Key.Month,
                DoanhThu = g.Sum(x => x.TongTien),
                SoHoaDon = g.Count()
            })
            .OrderBy(x => x.Nam).ThenBy(x => x.Thang)
            .ToListAsync(ct);

        // ── Cơ hội bán hàng: tổng, thắng, thua, tỉ lệ ──
        var coHoiQuery = _context.BhCoHoiBanHangs.AsNoTracking().Where(x => !x.IsDeleted);
        var tongSoCoHoi = await coHoiQuery.CountAsync(ct);
        var soThanhCong = await coHoiQuery.CountAsync(x => x.GiaiDoan == "ThanhCong", ct);
        var soThatBai = await coHoiQuery.CountAsync(x => x.GiaiDoan == "ThatBai", ct);
        var mauSoTyLe = soThanhCong + soThatBai;
        var tyLeThang = mauSoTyLe == 0 ? 0m : Math.Round(100m * soThanhCong / mauSoTyLe, 1);

        // ── Top 5 sản phẩm bán chạy (dựa trên giao dịch xuất bán trong kho) ──
        var top5SanPham = await _context.KhoTheKhos
            .AsNoTracking()
            .Where(k => k.LoaiGiaoDich == StockTransactionType.XuatBan && k.NgayGiaoDich >= tuNgay)
            .GroupBy(k => k.SanPham_Id)
            .Select(g => new { SanPhamId = g.Key, SoLuong = -g.Sum(x => x.SoLuongThayDoi) }) // xuất bán là số âm
            .OrderByDescending(x => x.SoLuong)
            .Take(5)
            .Join(_context.BhSanPhams.AsNoTracking(), t => t.SanPhamId, sp => sp.Id,
                (t, sp) => new SanPhamBanChayDto { SanPhamId = sp.Id, TenSanPham = sp.TenSP, SoLuongBan = t.SoLuong })
            .ToListAsync(ct);

        // ── Ticket hỗ trợ ──
        var ticketQuery = _context.TkTickets.AsNoTracking().Where(x => !x.IsDeleted);
        var tongSoTicket = await ticketQuery.CountAsync(ct);
        var soTicketDangMo = await ticketQuery.CountAsync(x => x.TrangThai != "Dong", ct);
        var soTicketKhanCap = await ticketQuery.CountAsync(x => x.MucDoUuTien == "KhanCap" && x.TrangThai != "Dong", ct);

        // ── Công nợ chưa thu (hóa đơn chưa hoàn tất thanh toán) ──
        var tongCongNo = await _context.KtHoaDons
            .AsNoTracking()
            .Where(h => h.TrangThaiThanhToan != "HoanTat")
            .SumAsync(h => h.TongTien - (h.SoTienDaThu ?? 0), ct);

        return new SalesAnalyticsDataDto
        {
            SoThangPhanTich = soThang,
            DoanhThuTheoThang = doanhThuTheoThang,
            TongSoCoHoi = tongSoCoHoi,
            SoCoHoiThanhCong = soThanhCong,
            SoCoHoiThatBai = soThatBai,
            TyLeThangCoHoi = tyLeThang,
            Top5SanPhamBanChay = top5SanPham,
            TongSoTicket = tongSoTicket,
            SoTicketDangMo = soTicketDangMo,
            SoTicketKhanCap = soTicketKhanCap,
            TongCongNoChuaThu = tongCongNo
        };
    }
}
