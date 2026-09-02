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

    public async Task<DashboardTrendsDto> GetDashboardTrendsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var dauThangNay = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var dauThangTruoc = dauThangNay.AddMonths(-1);

        // Trước đây: SELECT toàn bộ cột CreatedAt của cả 4 bảng (không giới hạn thời gian) về
        // App rồi mới đếm bằng LINQ-to-Objects — tải về TOÀN BỘ lịch sử từ ngày đầu vận hành
        // chỉ để đếm 2 con số mỗi bảng. Thay bằng CountAsync có điều kiện WHERE ngay trong SQL
        // (dịch thành COUNT(*) ... WHERE CreatedAt BETWEEN ...) — DB trả về đúng 1 số nguyên
        // mỗi câu thay vì hàng nghìn/hàng triệu dòng theo thời gian dữ liệu phình ra.
        //
        // Await tuần tự (KHÔNG Task.WhenAll): MySqlConnector chỉ cho phép 1 lệnh tại một thời
        // điểm trên cùng một connection/DbContext (không có MARS như SQL Server) — chạy song
        // song ở đây sẽ ném "A second operation was started on this context before a previous
        // operation completed". Vẫn nhanh hơn nhiều bản cũ vì mỗi câu chỉ là 1 COUNT(*) có index.
        var khachHangMoiThangNay = await _context.KhKhachHangs.AsNoTracking()
            .CountAsync(x => !x.IsDeleted && x.CreatedAt != null && x.CreatedAt >= dauThangNay, ct);
        var khachHangMoiThangTruoc = await _context.KhKhachHangs.AsNoTracking()
            .CountAsync(x => !x.IsDeleted && x.CreatedAt != null && x.CreatedAt >= dauThangTruoc && x.CreatedAt < dauThangNay, ct);

        var hopDongMoiThangNay = await _context.HdHopDongs.AsNoTracking()
            .CountAsync(x => x.CreatedAt != null && x.CreatedAt >= dauThangNay, ct);
        var hopDongMoiThangTruoc = await _context.HdHopDongs.AsNoTracking()
            .CountAsync(x => x.CreatedAt != null && x.CreatedAt >= dauThangTruoc && x.CreatedAt < dauThangNay, ct);

        var baoGiaMoiThangNay = await _context.HdBaoGias.AsNoTracking()
            .CountAsync(x => x.CreatedAt != null && x.CreatedAt >= dauThangNay, ct);
        var baoGiaMoiThangTruoc = await _context.HdBaoGias.AsNoTracking()
            .CountAsync(x => x.CreatedAt != null && x.CreatedAt >= dauThangTruoc && x.CreatedAt < dauThangNay, ct);

        var ticketMoiThangNay = await _context.TkTickets.AsNoTracking()
            .CountAsync(x => !x.IsDeleted && x.CreatedAt != null && x.CreatedAt >= dauThangNay, ct);
        var ticketMoiThangTruoc = await _context.TkTickets.AsNoTracking()
            .CountAsync(x => !x.IsDeleted && x.CreatedAt != null && x.CreatedAt >= dauThangTruoc && x.CreatedAt < dauThangNay, ct);

        return new DashboardTrendsDto
        {
            KhachHangMoiThangNay = khachHangMoiThangNay,
            KhachHangMoiThangTruoc = khachHangMoiThangTruoc,
            HopDongMoiThangNay = hopDongMoiThangNay,
            HopDongMoiThangTruoc = hopDongMoiThangTruoc,
            BaoGiaMoiThangNay = baoGiaMoiThangNay,
            BaoGiaMoiThangTruoc = baoGiaMoiThangTruoc,
            TicketMoiThangNay = ticketMoiThangNay,
            TicketMoiThangTruoc = ticketMoiThangTruoc
        };
    }

    public async Task<ChiSummaryDto> GetChiSummaryAsync(DateTime? tuNgay, DateTime? denNgay, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var dauThangNay = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var chiQueryGoc = _context.KtPhieuThuChis.AsNoTracking()
            .Where(x => x.LoaiPhieu == PaymentVoucherType.Chi);

        var chiThangNayQuery = chiQueryGoc
            .Where(x => x.NgayTao != null && x.NgayTao >= dauThangNay);
        var tongChiThangNay = await chiThangNayQuery.SumAsync(x => (decimal?)x.SoTien, ct) ?? 0m;
        var soPhieuChiThangNay = await chiThangNayQuery.CountAsync(ct);

        // Áp bộ lọc thời gian (nếu có) cho tổng + top khách hàng — không truyền gì thì mặc định
        // toàn thời gian, giữ đúng hành vi cũ.
        var chiQuery = chiQueryGoc;
        if (tuNgay.HasValue)
            chiQuery = chiQuery.Where(x => x.NgayTao != null && x.NgayTao >= tuNgay.Value.Date);
        if (denNgay.HasValue)
        {
            // denNgay đến từ <input type="date"> ở FE nên luôn là 00:00:00 của ngày đó — so sánh
            // "<=" sẽ VÔ TÌNH LOẠI cả những phiếu tạo trong chính ngày denNgay (VD: 15h00 ngày
            // 31/12 sẽ bị loại nếu lọc "đến 31/12"). Phải so với đầu ngày HÔM SAU mới bao trọn
            // hết ngày denNgay.
            var denNgayExclusive = denNgay.Value.Date.AddDays(1);
            chiQuery = chiQuery.Where(x => x.NgayTao != null && x.NgayTao < denNgayExclusive);
        }

        var tongTheoBoLoc = await chiQuery.SumAsync(x => (decimal?)x.SoTien, ct) ?? 0m;
        var soPhieuTheoBoLoc = await chiQuery.CountAsync(ct);

        // Top khách hàng phát sinh chi phí nhiều nhất trong CÙNG khoảng thời gian đang lọc
        // (chỉ tính phiếu có gắn khách hàng)
        var topKhachHang = await chiQuery
            .Where(x => x.KhachHang_Id != null)
            .GroupBy(x => x.KhachHang_Id!.Value)
            .Select(g => new { KhachHangId = g.Key, TongChi = g.Sum(x => x.SoTien), SoPhieu = g.Count() })
            .OrderByDescending(x => x.TongChi)
            .Take(5)
            .Join(_context.KhKhachHangs.AsNoTracking(), t => t.KhachHangId, kh => kh.Id,
                (t, kh) => new ChiTheoKhachHangDto
                {
                    KhachHangId = t.KhachHangId,
                    TenKhachHang = kh.TenKhachHang,
                    TongChi = t.TongChi,
                    SoPhieu = t.SoPhieu
                })
            .ToListAsync(ct);

        return new ChiSummaryDto
        {
            TongChiThangNay = tongChiThangNay,
            SoPhieuChiThangNay = soPhieuChiThangNay,
            TongChiTheoBoLoc = tongTheoBoLoc,
            SoPhieuChiTheoBoLoc = soPhieuTheoBoLoc,
            TuNgay = tuNgay,
            DenNgay = denNgay,
            TopKhachHangPhatSinhChi = topKhachHang
        };
    }
}