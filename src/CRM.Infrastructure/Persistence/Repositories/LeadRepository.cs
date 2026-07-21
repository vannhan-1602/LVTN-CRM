using CRM.Application.Common.Models;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly CrmDbContext _context;
    public LeadRepository(CrmDbContext context) => _context = context;

    public async Task<Lead?> GetByIdAsync(ulong id, bool includeDeleted = false, CancellationToken ct = default)
    {
        var e = await _context.Set<KhLeadEntity>()
            .FirstOrDefaultAsync(x => x.Id == id && (includeDeleted || !x.IsDeleted), ct);
        return e is null ? null : MapToDomain(e);
    }

    public async Task<PagedResult<Lead>> GetPagedAsync(
        int pageNumber, int pageSize, string? search, uint? ownerUserId,
        bool? isDeleted = null, string? tinhTrang = null, CancellationToken ct = default)
    {
        var query = _context.Set<KhLeadEntity>().AsNoTracking()
            .Where(x => x.IsDeleted == (isDeleted ?? false));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x =>
                x.TenLead.Contains(search) ||
                (x.TenCongTy != null && x.TenCongTy.Contains(search)));

        if (!string.IsNullOrWhiteSpace(tinhTrang))
            query = query.Where(x => x.TinhTrang == tinhTrang);

        // Sale chỉ thấy Lead mình phụ trách (NhanVienPhuTrach_Id tham chiếu HT_User.Id)
        if (ownerUserId.HasValue)
            query = query.Where(x => x.NhanVienPhuTrach_Id == ownerUserId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Lead>
        {
            Items = items.Select(MapToDomain).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<Lead> AddAsync(Lead lead, CancellationToken ct = default)
    {
        var entity = MapToEntity(lead);
        await _context.Set<KhLeadEntity>().AddAsync(entity, ct);
        lead.Id = entity.Id;
        return lead;
    }

    public async Task UpdateAsync(Lead lead, CancellationToken ct = default)
    {
        var entity = await _context.Set<KhLeadEntity>().FindAsync([lead.Id], ct);
        if (entity is null) return;
        entity.TenLead = lead.TenLead;
        entity.TenCongTy = lead.TenCongTy;
        entity.SoDienThoai = lead.SoDienThoai;
        entity.Email = lead.Email;
        entity.NguonLead = lead.NguonLead; // Bổ sung cập nhật nguồn lead
        entity.TinhTrang = lead.TinhTrang;
        entity.NhanVienPhuTrach_Id = lead.NhanVienPhuTrachId;
        entity.UpdatedAt = lead.UpdatedAt;
        // Không SaveChanges ở đây — Handler gọi qua IUnitOfWork để giữ 1 transaction/Command.
    }

    public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
    {
        var entity = await _context.Set<KhLeadEntity>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return false;
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public async Task<bool> RestoreAsync(ulong id, CancellationToken ct = default)
    {
        var entity = await _context.Set<KhLeadEntity>()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted, ct);
        if (entity is null) return false;
        entity.IsDeleted = false;
        entity.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    private static Lead MapToDomain(KhLeadEntity e) => new()
    {
        Id = e.Id,
        TenLead = e.TenLead,
        TenCongTy = e.TenCongTy,
        SoDienThoai = e.SoDienThoai,
        Email = e.Email,
        NguonLead = e.NguonLead ?? "Manual",
        TinhTrang = e.TinhTrang ?? LeadTinhTrang.Moi,
        NhanVienPhuTrachId = e.NhanVienPhuTrach_Id,
        IsDeleted = e.IsDeleted,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static KhLeadEntity MapToEntity(Lead lead) => new()
    {
        TenLead = lead.TenLead,
        TenCongTy = lead.TenCongTy,
        SoDienThoai = lead.SoDienThoai,
        Email = lead.Email,
        NguonLead = lead.NguonLead ?? "Manual",
        TinhTrang = lead.TinhTrang,
        NhanVienPhuTrach_Id = lead.NhanVienPhuTrachId,
        IsDeleted = lead.IsDeleted,
        CreatedAt = lead.CreatedAt,
        UpdatedAt = lead.UpdatedAt
    };
}