using CRM.Application.Common.Models;
using CRM.Application.Features.AuditLog.DTOs;

namespace CRM.Application.Interfaces.Audit;

public interface IAuditLogReader
{
    Task<PagedResult<AuditLogDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? tableName,
        string? action,
        uint? userId,
        ulong? recordId,
        DateTime? tuNgay,
        DateTime? denNgay,
        CancellationToken ct = default);

    /// <summary>Danh sách các TableName đã từng xuất hiện trong audit log — dùng cho dropdown lọc.</summary>
    Task<List<string>> GetDistinctTableNamesAsync(CancellationToken ct = default);
}
