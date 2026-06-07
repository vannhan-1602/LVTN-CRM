using CRM.Application.Common.Models;
using CRM.Domain.Entities.Customers;

namespace CRM.Application.Interfaces.Customers;

public interface ICustomerRepository
{
    Task<KhachHang?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<PagedResult<KhachHang>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);
    Task<KhachHang> AddAsync(KhachHang customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(KhachHang customer, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(ulong id, CancellationToken cancellationToken = default);
    Task<bool> ExistsMaKhachHangAsync(string maKhachHang, ulong? excludeId = null, CancellationToken cancellationToken = default);
    Task<string> GenerateMaKhachHangAsync(CancellationToken cancellationToken = default);
    Task<bool> LoaiKhachHangExistsAsync(ushort id, CancellationToken cancellationToken = default);
    Task<bool> TinhTrangKhachHangExistsAsync(ushort id, CancellationToken cancellationToken = default);
    Task<KhachHang?> GetByMaKhachHangAsync(string maKhachHang, CancellationToken cancellationToken = default);
}
