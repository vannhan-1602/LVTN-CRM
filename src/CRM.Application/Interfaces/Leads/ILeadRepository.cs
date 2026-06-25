using CRM.Application.Common.Models;
using CRM.Domain.Entities.Customers;

namespace CRM.Application.Interfaces.Leads;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default);

    Task<PagedResult<Lead>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        //  khi có giá trị, chỉ trả về Lead có NhanVienPhuTrachId == giá trị này (HT_User.Id).
        // null nghĩa là không giới hạn (dùng cho Manager xem toàn đội).
        uint? ownerUserId,
        CancellationToken cancellationToken = default);

    Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default);
    Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(ulong id, CancellationToken cancellationToken = default);
}