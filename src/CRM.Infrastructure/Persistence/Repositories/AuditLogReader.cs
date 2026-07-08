using CRM.Application.Common.Models;
using CRM.Application.Features.AuditLog.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class AuditLogReader : IAuditLogReader
{
    private readonly CrmDbContext _context;
    public AuditLogReader(CrmDbContext context) => _context = context;

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? tableName, string? action,
        uint? userId, ulong? recordId, DateTime? tuNgay, DateTime? denNgay,
        CancellationToken ct = default)
    {
        var query =
            from log in _context.SysAuditLogs.AsNoTracking()
            join u in _context.HtUsers on log.UserId equals (uint?)u.Id into userJoin
            from u in userJoin.DefaultIfEmpty()
            join ns in _context.HtThongTinNhanSu on u.NhanSuId equals (uint?)ns.Id into nsJoin
            from ns in nsJoin.DefaultIfEmpty()
            select new
            {
                Log = log,
                Username = u != null ? u.Username : null,
                HoTen = ns != null ? ns.HoTen : null
            };

        if (!string.IsNullOrWhiteSpace(tableName))
            query = query.Where(x => x.Log.TableName == tableName);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Log.Action == action);

        if (userId.HasValue)
            query = query.Where(x => x.Log.UserId == userId.Value);

        if (recordId.HasValue)
            query = query.Where(x => x.Log.RecordId == recordId.Value);

        if (tuNgay.HasValue)
            query = query.Where(x => x.Log.ChangedAt >= tuNgay.Value);

        if (denNgay.HasValue)
            query = query.Where(x => x.Log.ChangedAt <= denNgay.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Log.ChangedAt)
            .ThenByDescending(x => x.Log.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogDto
            {
                Id = x.Log.Id,
                TableName = x.Log.TableName,
                RecordId = x.Log.RecordId,
                Action = x.Log.Action,
                OldData = x.Log.OldData,
                NewData = x.Log.NewData,
                UserId = x.Log.UserId,
                TenNguoiThucHien = x.HoTen,
                UsernameNguoiThucHien = x.Username,
                ChangedAt = x.Log.ChangedAt
            })
            .ToListAsync(ct);

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<string>> GetDistinctTableNamesAsync(CancellationToken ct = default) =>
        await _context.SysAuditLogs.AsNoTracking()
            .Select(x => x.TableName)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(ct);
}
