using CRM.Application.Common.Models;
using CRM.Application.Features.Opportunities.DTOs;
using CRM.Application.Interfaces.Opportunities;
using CRM.Domain.Entities.Sales;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class OpportunityRepository : IOpportunityRepository
{
    private readonly CrmDbContext _ctx;
    public OpportunityRepository(CrmDbContext ctx) => _ctx = ctx;

    public async Task<CoHoiBanHang?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        // Chỉ đọc để trả về domain object hiển thị — UpdateAsync/SoftDeleteAsync bên dưới tự
        // FindAsync lại (luôn tracked) nên AsNoTracking ở đây an toàn, giảm overhead change-tracker.
        var e = await _ctx.Set<BhCoHoiBanHangEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return e is null ? null : ToDomain(e);
    }

    public async Task<OpportunityDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default)
    {
        var q = BuildEnrichedQuery().Where(x => x.O.Id == id && !x.O.IsDeleted);
        var r = await q.FirstOrDefaultAsync(ct);
        return r is null ? null : ToDto(r.O, r.TenKhachHang, r.TenLead, r.TenNhanVien);
    }

    public async Task<PagedResult<OpportunityDto>> GetPagedAsync(
        int pageNumber, int pageSize,
        string? search, string? giaiDoan,
        ulong? khachHangId, ulong? leadId,
        uint? ownerUserId, CancellationToken ct = default)
    {
        var q = BuildEnrichedQuery().Where(x => !x.O.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(x => x.O.TenThuongVu.Contains(search));
        if (!string.IsNullOrWhiteSpace(giaiDoan))
            q = q.Where(x => x.O.GiaiDoan == giaiDoan);
        if (khachHangId.HasValue)
            q = q.Where(x => x.O.KhachHang_Id == khachHangId.Value);
        if (leadId.HasValue)
            q = q.Where(x => x.O.Lead_Id == leadId.Value);
        if (ownerUserId.HasValue)
            q = q.Where(x => x.O.NhanVienPhuTrach_Id == (int)ownerUserId.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.O.UpdatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<OpportunityDto>
        {
            Items = items.Select(r => ToDto(r.O, r.TenKhachHang, r.TenLead, r.TenNhanVien)).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<CoHoiBanHang> AddAsync(CoHoiBanHang d, CancellationToken ct = default)
    {
        var e = ToEntity(d);
        _ctx.Set<BhCoHoiBanHangEntity>().Add(e);
        await _ctx.SaveChangesAsync(ct);
        d.Id = e.Id;
        return d;
    }

    public async Task UpdateAsync(CoHoiBanHang d, CancellationToken ct = default)
    {
        var e = await _ctx.Set<BhCoHoiBanHangEntity>().FindAsync([d.Id], ct);
        if (e is null) return;
        e.TenThuongVu = d.TenThuongVu;
        e.GiaiDoan = d.GiaiDoan;
        e.KhachHang_Id = d.KhachHangId;
        e.Lead_Id = d.LeadId;
        e.TyLeThanhCong = d.TyLeThanhCong;
        e.DoanhThuKyVong = d.DoanhThuKyVong;
        e.GhiChu = d.GhiChu;
        e.NgayDuKien = d.NgayDuKien;
        e.NhanVienPhuTrach_Id = d.NhanVienPhuTrachId;
        e.UpdatedAt = d.UpdatedAt;
    }

    public async Task<bool> SoftDeleteAsync(ulong id, CancellationToken ct = default)
    {
        var e = await _ctx.Set<BhCoHoiBanHangEntity>().FindAsync([id], ct);
        if (e is null) return false;
        e.IsDeleted = true;
        e.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public async Task ReassignLeadOpportunitiesToCustomerAsync(ulong leadId, ulong customerId, CancellationToken ct = default)
    {
        var items = await _ctx.Set<BhCoHoiBanHangEntity>()
            .Where(x => x.Lead_Id == leadId && !x.IsDeleted)
            .ToListAsync(ct);

        foreach (var e in items)
        {
            e.KhachHang_Id = customerId;
            e.Lead_Id = null;
            e.UpdatedAt = DateTime.UtcNow;
        }
        // Không SaveChanges ở đây — theo UnitOfWork của caller.
    }

    public async Task<OpportunitySummaryDto> GetSummaryAsync(uint? ownerUserId, CancellationToken ct = default)
    {
        // Trước đây: load TOÀN BỘ cơ hội bán hàng vào memory (ToListAsync không lọc) rồi mới
        // Count/Sum/Average bằng LINQ-to-Objects — với vài nghìn bản ghi vẫn ổn, nhưng không
        // scale (tốn RAM + băng thông DB->App tuyến tính theo số dòng, dù chỉ cần vài con số).
        // Đẩy toàn bộ phép tính xuống SQL (GroupBy/Sum/Average dịch thành 1 câu SQL) để DB tính
        // aggregate tại chỗ, chỉ trả về đúng số dòng theo GiaiDoan.
        var q = _ctx.Set<BhCoHoiBanHangEntity>().AsNoTracking().Where(x => !x.IsDeleted);
        if (ownerUserId.HasValue)
            q = q.Where(x => x.NhanVienPhuTrach_Id == (int)ownerUserId.Value);

        var byStage = await q
            .GroupBy(x => x.GiaiDoan)
            .Select(g => new
            {
                GiaiDoan = g.Key,
                Count = g.Count(),
                TongDoanhThuKyVong = g.Sum(x => x.DoanhThuKyVong ?? 0),
                TongTyLe = g.Sum(x => (double)x.TyLeThanhCong)
            })
            .ToListAsync(ct);

        var countByStage = byStage.ToDictionary(g => g.GiaiDoan, g => g.Count);
        var thanhCong = byStage.FirstOrDefault(g => g.GiaiDoan == "ThanhCong");
        var thatBai = byStage.FirstOrDefault(g => g.GiaiDoan == "ThatBai");

        var totalCount = byStage.Sum(g => g.Count);
        var activeCount = totalCount - (thanhCong?.Count ?? 0) - (thatBai?.Count ?? 0);

        return new OpportunitySummaryDto
        {
            TotalActive = activeCount,
            ThanhCong = thanhCong?.Count ?? 0,
            ThatBai = thatBai?.Count ?? 0,
            TotalDoanhThuKyVong = byStage.Sum(g => g.TongDoanhThuKyVong),
            DoanhThuThanhCong = thanhCong?.TongDoanhThuKyVong ?? 0,
            CountByStage = countByStage,
            TyLeThanhCongTrungBinh = totalCount > 0
                ? byStage.Sum(g => g.TongTyLe) / totalCount
                : 0
        };
    }


    private IQueryable<OpportunityRow> BuildEnrichedQuery() =>
        from o in _ctx.Set<BhCoHoiBanHangEntity>().AsNoTracking()
        join kh in _ctx.KhKhachHangs on o.KhachHang_Id equals (ulong?)kh.Id into khJ
        from kh in khJ.DefaultIfEmpty()
        join lead in _ctx.KhLeads on o.Lead_Id equals (ulong?)lead.Id into lJ
        from lead in lJ.DefaultIfEmpty()
        join u in _ctx.HtUsers on o.NhanVienPhuTrach_Id equals (int?)u.Id into uJ
        from u in uJ.DefaultIfEmpty()
        join ns in _ctx.HtThongTinNhanSu on u.NhanSuId equals (uint?)ns.Id into nsJ
        from ns in nsJ.DefaultIfEmpty()
        select new OpportunityRow
        {
            O = o,
            TenKhachHang = kh != null ? kh.TenKhachHang : null,
            TenLead = lead != null ? lead.TenLead : null,
            TenNhanVien = ns != null ? ns.HoTen : (u != null ? u.Username : null)
        };

    private class OpportunityRow
    {
        public BhCoHoiBanHangEntity O { get; set; } = null!;
        public string? TenKhachHang { get; set; }
        public string? TenLead { get; set; }
        public string? TenNhanVien { get; set; }
    }

    private static CoHoiBanHang ToDomain(BhCoHoiBanHangEntity e) => new()
    {
        Id = e.Id,
        TenThuongVu = e.TenThuongVu,
        GiaiDoan = e.GiaiDoan,
        KhachHangId = e.KhachHang_Id,
        LeadId = e.Lead_Id,
        TyLeThanhCong = e.TyLeThanhCong,
        DoanhThuKyVong = e.DoanhThuKyVong,
        GhiChu = e.GhiChu,
        NgayDuKien = e.NgayDuKien,
        NhanVienPhuTrachId = e.NhanVienPhuTrach_Id,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static BhCoHoiBanHangEntity ToEntity(CoHoiBanHang d) => new()
    {
        TenThuongVu = d.TenThuongVu,
        GiaiDoan = d.GiaiDoan,
        KhachHang_Id = d.KhachHangId,
        Lead_Id = d.LeadId,
        TyLeThanhCong = d.TyLeThanhCong,
        DoanhThuKyVong = d.DoanhThuKyVong,
        GhiChu = d.GhiChu,
        NgayDuKien = d.NgayDuKien,
        NhanVienPhuTrach_Id = d.NhanVienPhuTrachId,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };

    private static OpportunityDto ToDto(BhCoHoiBanHangEntity e, string? tenKh, string? tenLead, string? tenNv) => new()
    {
        Id = e.Id,
        TenThuongVu = e.TenThuongVu,
        GiaiDoan = e.GiaiDoan,
        KhachHangId = e.KhachHang_Id,
        TenKhachHang = tenKh,
        LeadId = e.Lead_Id,
        TenLead = tenLead,
        TyLeThanhCong = e.TyLeThanhCong,
        DoanhThuKyVong = e.DoanhThuKyVong,
        GhiChu = e.GhiChu,
        NgayDuKien = e.NgayDuKien,
        NhanVienPhuTrachId = e.NhanVienPhuTrach_Id != null ? (int?)e.NhanVienPhuTrach_Id : null,
        TenNhanVien = tenNv,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}