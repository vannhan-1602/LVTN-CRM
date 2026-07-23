using CRM.Application.Features.Alerts.DTOs;
using CRM.Application.Interfaces.Alerts;
using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class AlertRepository : IAlertRepository
{
    // Giới hạn số bản ghi trả về mỗi nhóm để Dashboard gọn — Count vẫn phản ánh tổng số thật.
    private const int SoLuongToiDaMoiNhom = 8;
    private const int SoGioSapToiHenXuLy = 48;

    private static readonly string[] TenLoaiTicketTuDong = ["Nhắc thanh toán", "Nhắc gia hạn hợp đồng"];

    private readonly CrmDbContext _context;
    public AlertRepository(CrmDbContext context) => _context = context;

    public async Task<DashboardAlertGroupDto> GetLeadsChuaPhanCongAsync(CancellationToken ct = default)
    {
        var query = _context.KhLeads.AsNoTracking()
            .Where(l => !l.IsDeleted && l.NhanVienPhuTrach_Id == null);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(SoLuongToiDaMoiNhom)
            .Select(l => new DashboardAlertDto
            {
                Type = "LeadChuaPhanCong",
                Severity = AlertSeverity.Warning,
                Title = l.TenLead,
                Description = "Lead" + (l.TenCongTy != null ? $" của công ty {l.TenCongTy}" : "") + " chưa được phân công nhân viên phụ trách.",
                EntityType = "Lead",
                EntityId = l.Id,
                DueAt = null
            })
            .ToListAsync(ct);

        return new DashboardAlertGroupDto
        {
            GroupKey = "leads_chua_phan_cong",
            GroupTitle = "Lead chưa phân công",
            Severity = AlertSeverity.Warning,
            Count = total,
            Items = items
        };
    }

    public async Task<DashboardAlertGroupDto> GetCustomersChuaPhanCongAsync(CancellationToken ct = default)
    {
        var query = _context.KhKhachHangs.AsNoTracking()
            .Where(k => !k.IsDeleted && k.NhanVienPhuTrachId == null);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(k => k.CreatedAt)
            .Take(SoLuongToiDaMoiNhom)
            .Select(k => new DashboardAlertDto
            {
                Type = "CustomerChuaPhanCong",
                Severity = AlertSeverity.Warning,
                Title = k.TenKhachHang,
                Description = $"Khách hàng {k.MaKhachHang} chưa được phân công nhân viên phụ trách.",
                EntityType = "Customer",
                EntityId = k.Id,
                DueAt = null
            })
            .ToListAsync(ct);

        return new DashboardAlertGroupDto
        {
            GroupKey = "customers_chua_phan_cong",
            GroupTitle = "Khách hàng chưa phân công",
            Severity = AlertSeverity.Warning,
            Count = total,
            Items = items
        };
    }

    public async Task<DashboardAlertGroupDto> GetTicketsChuaPhanCongAsync(CancellationToken ct = default)
    {
        var query = _context.TkTickets.AsNoTracking()
            .Where(t => !t.IsDeleted && t.TrangThai != "Dong"
                        && t.NhanVienTiepNhan_Id == null && t.NhanVienXuLy_Id == null);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.MucDoUuTien == "KhanCap")
            .ThenByDescending(t => t.CreatedAt)
            .Take(SoLuongToiDaMoiNhom)
            .Select(t => new DashboardAlertDto
            {
                Type = "TicketChuaPhanCong",
                Severity = t.MucDoUuTien == "KhanCap" ? AlertSeverity.Danger : AlertSeverity.Warning,
                Title = $"[{t.MaTicket}] {t.TieuDe}",
                Description = $"Ticket chưa được phân công nhân viên xử lý (mức độ: {t.MucDoUuTien}).",
                EntityType = "Ticket",
                EntityId = t.Id,
                DueAt = t.ThoiHanSLA
            })
            .ToListAsync(ct);

        return new DashboardAlertGroupDto
        {
            GroupKey = "ticket_chua_phan_cong",
            GroupTitle = "Ticket chưa phân công",
            Severity = AlertSeverity.Warning,
            Count = total,
            Items = items
        };
    }

    public async Task<DashboardAlertGroupDto> GetTicketKhanCapAsync(uint? nhanVienId, CancellationToken ct = default)
    {
        var query = _context.TkTickets.AsNoTracking()
            .Where(t => !t.IsDeleted && t.TrangThai != "Dong" && t.MucDoUuTien == "KhanCap");

        if (nhanVienId is not null)
            query = query.Where(t => t.NhanVienXuLy_Id == nhanVienId || t.NhanVienTiepNhan_Id == nhanVienId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.ThoiHanSLA ?? DateTime.MaxValue)
            .Take(SoLuongToiDaMoiNhom)
            .Select(t => new DashboardAlertDto
            {
                Type = "TicketKhanCap",
                Severity = AlertSeverity.Danger,
                Title = $"[{t.MaTicket}] {t.TieuDe}",
                Description = t.ThoiHanSLA != null
                    ? $"Ticket khẩn cấp — hạn xử lý SLA: {t.ThoiHanSLA:dd/MM/yyyy HH:mm}."
                    : "Ticket khẩn cấp đang chờ xử lý.",
                EntityType = "Ticket",
                EntityId = t.Id,
                DueAt = t.ThoiHanSLA
            })
            .ToListAsync(ct);

        return new DashboardAlertGroupDto
        {
            GroupKey = "ticket_khan_cap",
            GroupTitle = "Ticket khẩn cấp",
            Severity = AlertSeverity.Danger,
            Count = total,
            Items = items
        };
    }

    public async Task<DashboardAlertGroupDto> GetTicketQuaHanSlaAsync(uint? nhanVienId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var query = _context.TkTickets.AsNoTracking()
            .Where(t => !t.IsDeleted && t.TrangThai != "Dong" && t.ThoiHanSLA != null && t.ThoiHanSLA < now);

        if (nhanVienId is not null)
            query = query.Where(t => t.NhanVienXuLy_Id == nhanVienId || t.NhanVienTiepNhan_Id == nhanVienId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.ThoiHanSLA)
            .Take(SoLuongToiDaMoiNhom)
            .Select(t => new DashboardAlertDto
            {
                Type = "TicketQuaHanSLA",
                Severity = AlertSeverity.Danger,
                Title = $"[{t.MaTicket}] {t.TieuDe}",
                Description = $"Đã quá hạn SLA từ {t.ThoiHanSLA:dd/MM/yyyy HH:mm} — đã escalate {t.SoLanEscalate} lần.",
                EntityType = "Ticket",
                EntityId = t.Id,
                DueAt = t.ThoiHanSLA
            })
            .ToListAsync(ct);

        return new DashboardAlertGroupDto
        {
            GroupKey = "ticket_qua_han_sla",
            GroupTitle = "Ticket quá hạn SLA",
            Severity = AlertSeverity.Danger,
            Count = total,
            Items = items
        };
    }

    public async Task<DashboardAlertGroupDto> GetTicketSapHenXuLyAsync(uint? nhanVienId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var nguong = now.AddHours(SoGioSapToiHenXuLy);

        var query = _context.TkTickets.AsNoTracking()
            .Where(t => !t.IsDeleted && t.TrangThai != "Dong"
                        && t.NgayHenXuLy != null && t.NgayHenXuLy >= now && t.NgayHenXuLy <= nguong);

        if (nhanVienId is not null)
            query = query.Where(t => t.NhanVienXuLy_Id == nhanVienId || t.NhanVienTiepNhan_Id == nhanVienId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.NgayHenXuLy)
            .Take(SoLuongToiDaMoiNhom)
            .Select(t => new DashboardAlertDto
            {
                Type = "TicketSapHenXuLy",
                Severity = AlertSeverity.Info,
                Title = $"[{t.MaTicket}] {t.TieuDe}",
                Description = $"Sắp đến giờ hẹn xử lý: {t.NgayHenXuLy:dd/MM/yyyy HH:mm}.",
                EntityType = "Ticket",
                EntityId = t.Id,
                DueAt = t.NgayHenXuLy
            })
            .ToListAsync(ct);

        return new DashboardAlertGroupDto
        {
            GroupKey = "ticket_sap_hen_xu_ly",
            GroupTitle = "Ticket sắp tới giờ hẹn xử lý",
            Severity = AlertSeverity.Info,
            Count = total,
            Items = items
        };
    }

    public async Task<DashboardAlertGroupDto> GetTicketNhacThanhToanGiaHanAsync(uint? nhanVienId, CancellationToken ct = default)
    {
        var loaiTicketIds = await _context.TkLoaiTickets.AsNoTracking()
            .Where(l => TenLoaiTicketTuDong.Contains(l.TenLoai))
            .Select(l => l.Id)
            .ToListAsync(ct);

        var query = _context.TkTickets.AsNoTracking()
            .Where(t => !t.IsDeleted && t.TrangThai != "Dong"
                        && t.LoaiTicket_Id != null && loaiTicketIds.Contains(t.LoaiTicket_Id.Value));

        if (nhanVienId is not null)
            query = query.Where(t => t.NhanVienTiepNhan_Id == nhanVienId || t.NhanVienXuLy_Id == nhanVienId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(SoLuongToiDaMoiNhom)
            .Select(t => new DashboardAlertDto
            {
                Type = "TicketNhacThanhToanGiaHan",
                Severity = AlertSeverity.Warning,
                Title = $"[{t.MaTicket}] {t.TieuDe}",
                Description = t.MoTa ?? string.Empty,
                EntityType = "Ticket",
                EntityId = t.Id,
                DueAt = t.NgayHenXuLy
            })
            .ToListAsync(ct);

        return new DashboardAlertGroupDto
        {
            GroupKey = "ticket_nhac_thanh_toan_gia_han",
            GroupTitle = "Nhắc thanh toán / gia hạn hợp đồng",
            Severity = AlertSeverity.Warning,
            Count = total,
            Items = items
        };
    }

    public async Task<DashboardAlertGroupDto> GetDotThanhToanCanXuLyAsync(CancellationToken ct = default)
    {
        var query = _context.HdLichThanhToans.AsNoTracking()
            .Include(x => x.HopDong)
            .Where(x => x.TrangThai == "ChoThanhToan" || x.TrangThai == "QuaHan");

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.HanThanhToan)
            .Take(SoLuongToiDaMoiNhom)
            .Select(x => new DashboardAlertDto
            {
                Type = "DotThanhToan",
                Severity = x.TrangThai == "QuaHan" ? AlertSeverity.Danger : AlertSeverity.Warning,
                Title = $"Đợt {x.SoDot} — Hợp đồng {(x.HopDong != null ? x.HopDong.MaHopDong : "")}",
                Description = x.TrangThai == "QuaHan"
                    ? $"Đã quá hạn thanh toán ({x.HanThanhToan:dd/MM/yyyy}), số tiền {x.SoTien:N0}đ."
                    : $"Sắp đến hạn thanh toán ({x.HanThanhToan:dd/MM/yyyy}), số tiền {x.SoTien:N0}đ.",
                EntityType = "Contract",
                EntityId = x.HopDong_Id,
                DueAt = x.HanThanhToan.ToDateTime(TimeOnly.MinValue)
            })
            .ToListAsync(ct);

        return new DashboardAlertGroupDto
        {
            GroupKey = "dot_thanh_toan",
            GroupTitle = "Đợt thanh toán cần xử lý",
            Severity = AlertSeverity.Warning,
            Count = total,
            Items = items
        };
    }

    public async Task<DashboardAlertGroupDto> GetHoaDonConNoAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Chỉ hóa đơn thanh toán 1 lần (không thuộc lịch trả góp) — hóa đơn thuộc lịch trả góp
        // đã được cảnh báo qua GetDotThanhToanCanXuLyAsync (dựa trên HanThanhToan cụ thể).
        var query =
            from hd in _context.KtHoaDons.AsNoTracking()
            join kh in _context.KhKhachHangs.AsNoTracking()
                on hd.KhachHang_Id equals kh.Id into khJoin
            from kh in khJoin.DefaultIfEmpty()
            where hd.TrangThaiThanhToan != "HoanTat" && hd.LichThanhToanId == null
            select new { HoaDon = hd, TenKhachHang = kh != null ? kh.TenKhachHang : null };

        var total = await query.CountAsync(ct);
        var raw = await query
            .OrderBy(x => x.HoaDon.CreatedAt)
            .Take(SoLuongToiDaMoiNhom)
            .ToListAsync(ct);

        var items = raw.Select(x =>
        {
            var soTienConLai = x.HoaDon.TongTien - (x.HoaDon.SoTienDaThu ?? 0m);
            var soNgayTonDong = x.HoaDon.CreatedAt != null ? (now - x.HoaDon.CreatedAt.Value).Days : 0;

            return new DashboardAlertDto
            {
                Type = "HoaDonConNo",
                Severity = soNgayTonDong >= 30 ? AlertSeverity.Danger : AlertSeverity.Warning,
                Title = $"{x.HoaDon.MaHoaDon}" + (x.TenKhachHang != null ? $" — {x.TenKhachHang}" : ""),
                Description = $"Còn nợ {soTienConLai:N0}đ, tồn đọng {soNgayTonDong} ngày kể từ ngày xuất hóa đơn.",
                EntityType = "Invoice",
                EntityId = x.HoaDon.Id,
                DueAt = x.HoaDon.CreatedAt
            };
        }).ToList();

        return new DashboardAlertGroupDto
        {
            GroupKey = "hoa_don_con_no",
            GroupTitle = "Hóa đơn còn nợ",
            Severity = AlertSeverity.Warning,
            Count = total,
            Items = items
        };
    }
}