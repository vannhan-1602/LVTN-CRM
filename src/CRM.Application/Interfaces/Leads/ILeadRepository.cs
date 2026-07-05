using CRM.Application.Common.Models;
using CRM.Domain.Entities.Customers;

namespace CRM.Application.Interfaces.Leads;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(ulong id, bool includeDeleted = false, CancellationToken cancellationToken = default);

    Task<PagedResult<Lead>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        //  khi có giá trị, chỉ trả về Lead có NhanVienPhuTrachId == giá trị này (HT_User.Id).
        // null nghĩa là không giới hạn (dùng cho Manager xem toàn đội).
        uint? ownerUserId,
        //  null/false = chỉ lấy chưa xóa (mặc định); true = chỉ lấy đã xóa (đã khóa)
        bool? isDeleted = null,
        CancellationToken cancellationToken = default);

    Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default);
    Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(ulong id, CancellationToken cancellationToken = default);
    Task<bool> RestoreAsync(ulong id, CancellationToken cancellationToken = default);
}