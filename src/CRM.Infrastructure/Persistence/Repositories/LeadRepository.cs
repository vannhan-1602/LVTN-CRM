using CRM.Application.Common.Models;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Persistence.Repositories
{
    public class LeadRepository : ILeadRepository
    {
        private readonly CrmDbContext _context;
        public LeadRepository(CrmDbContext context) => _context = context;

        public async Task<Lead?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var e = await _context.Set<KhLeadEntity>().FindAsync([id], ct);
            return e is null ? null : MapToDomain(e);
        }

        public async Task<PagedResult<Lead>> GetPagedAsync(
            int pageNumber, int pageSize, string? search, CancellationToken ct = default)
        {
            var query = _context.Set<KhLeadEntity>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x =>
                    x.TenLead.Contains(search) ||
                    (x.TenCongTy != null && x.TenCongTy.Contains(search)));

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
            _context.Set<KhLeadEntity>().Add(entity);
            await _context.SaveChangesAsync(ct);
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
            entity.TinhTrang = lead.TinhTrang;
            entity.NhanVienPhuTrach_Id = lead.NhanVienPhuTrachId;
            entity.UpdatedAt = lead.UpdatedAt;
        }

        public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _context.Set<KhLeadEntity>().FindAsync([id], ct);
            if (entity is null) return false;
            _context.Set<KhLeadEntity>().Remove(entity);
            return true;
        }

        private static Lead MapToDomain(KhLeadEntity e) => new()
        {
            Id = e.Id,
            TenLead = e.TenLead,
            TenCongTy = e.TenCongTy,
            SoDienThoai = e.SoDienThoai,
            Email = e.Email,
            TinhTrang = e.TinhTrang ?? LeadTinhTrang.Moi,
            NhanVienPhuTrachId = e.NhanVienPhuTrach_Id,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };

        private static KhLeadEntity MapToEntity(Lead lead) => new()
        {
            TenLead = lead.TenLead,
            TenCongTy = lead.TenCongTy,
            SoDienThoai = lead.SoDienThoai,
            Email = lead.Email,
            TinhTrang = lead.TinhTrang,
            NhanVienPhuTrach_Id = lead.NhanVienPhuTrachId,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }
}
