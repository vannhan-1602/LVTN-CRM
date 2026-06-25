using CRM.Application.Features.Activities.DTOs;
using CRM.Application.Interfaces.Activities;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly CrmDbContext _ctx;
    public ActivityRepository(CrmDbContext ctx) => _ctx = ctx;

    public async Task<List<ActivityDto>> GetByKhachHangAsync(ulong khachHangId, CancellationToken ct = default)
    {
        var rows = await BuildEnrichedQuery()
            .Where(x => x.A.KhachHang_Id == khachHangId)
            .OrderByDescending(x => x.A.ThoiGianThucHien)
            .ToListAsync(ct);

        return rows.Select(r => ToDto(r.A, r.TenKhachHang, r.TenNhanVien)).ToList();
    }

    public async Task<List<ActivityDto>> GetByLeadAsync(ulong leadId, CancellationToken ct = default)
    {
        var rows = await BuildEnrichedQuery()
            .Where(x => x.A.Lead_Id == leadId)
            .OrderByDescending(x => x.A.ThoiGianThucHien)
            .ToListAsync(ct);

        return rows.Select(r => ToDto(r.A, r.TenKhachHang, r.TenNhanVien)).ToList();
    }

    public async Task<ActivityDto?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var row = await BuildEnrichedQuery()
            .Where(x => x.A.Id == id)
            .FirstOrDefaultAsync(ct);

        return row is null ? null : ToDto(row.A, row.TenKhachHang, row.TenNhanVien);
    }

    public async Task<ActivityDto> AddAsync(
        ulong? khachHangId,
        ulong? leadId,
        string loaiHoatDong,
        string? noiDung,
        DateTime thoiGianThucHien,
        uint? nhanVienId,
        CancellationToken ct = default)
    {
        var e = new KhHoatDongEntity
        {
            KhachHang_Id = khachHangId,
            Lead_Id = leadId,
            LoaiHoatDong = loaiHoatDong,
            NoiDung = noiDung,
            ThoiGianThucHien = thoiGianThucHien,
            NhanVien_Id = nhanVienId,
            CreatedAt = DateTime.UtcNow
        };
        _ctx.Set<KhHoatDongEntity>().Add(e);
        await _ctx.SaveChangesAsync(ct);
        return (await GetByIdAsync(e.Id, ct))!;
    }

    public async Task<ActivityDto> UpdateAsync(
        ulong id,
        string loaiHoatDong,
        string? noiDung,
        DateTime thoiGianThucHien,
        CancellationToken ct = default)
    {
        var e = await _ctx.Set<KhHoatDongEntity>().FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Activity {id} not found.");

        e.LoaiHoatDong = loaiHoatDong;
        e.NoiDung = noiDung;
        e.ThoiGianThucHien = thoiGianThucHien;
        e.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
    {
        var e = await _ctx.Set<KhHoatDongEntity>().FindAsync([id], ct);
        if (e is null) return false;
        _ctx.Set<KhHoatDongEntity>().Remove(e);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    private IQueryable<ActivityRow> BuildEnrichedQuery() =>
        from a in _ctx.Set<KhHoatDongEntity>().AsNoTracking()
        join kh in _ctx.KhKhachHangs on a.KhachHang_Id equals (ulong?)kh.Id into khJ
        from kh in khJ.DefaultIfEmpty()
        join u in _ctx.HtUsers on a.NhanVien_Id equals (uint?)u.Id into uJ
        from u in uJ.DefaultIfEmpty()
        select new ActivityRow
        {
            A = a,
            TenKhachHang = kh != null ? kh.TenKhachHang : null,
            TenNhanVien = u != null ? u.Username : null
        };

    private class ActivityRow
    {
        public KhHoatDongEntity A { get; set; } = null!;
        public string? TenKhachHang { get; set; }
        public string? TenNhanVien { get; set; }
    }

    private static ActivityDto ToDto(KhHoatDongEntity a, string? tenKh, string? tenNv) => new()
    {
        Id = a.Id,
        KhachHangId = a.KhachHang_Id,
        TenKhachHang = tenKh,
        LeadId = a.Lead_Id,
        LoaiHoatDong = a.LoaiHoatDong,
        NoiDung = a.NoiDung,
        ThoiGianThucHien = a.ThoiGianThucHien,
        NhanVienId = a.NhanVien_Id,
        TenNhanVien = tenNv,
        CreatedAt = a.CreatedAt
    };
}