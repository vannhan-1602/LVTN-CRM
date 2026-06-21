using CRM.Application.Common.Models;
using CRM.Application.Features.Customers.DTOs;
using CRM.Domain.Entities.Customers;

namespace CRM.Application.Interfaces.Customers;

public interface ICustomerRepository
{
    Task<KhachHang?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByIdEnrichedAsync(ulong id, CancellationToken cancellationToken = default);

    Task<PagedResult<CustomerDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        ushort? loaiKhachHangId,
        ushort? tinhTrangId,
        //  null = không giới hạn (Manager); có giá trị = chỉ Customer của Sale đó
        uint? ownerNhanSuId,
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
