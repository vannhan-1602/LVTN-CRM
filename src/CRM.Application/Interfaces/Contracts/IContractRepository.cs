using CRM.Application.Common.Models;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Domain.Entities.Sales;

namespace CRM.Application.Interfaces.Contracts;

public interface IContractRepository
{
    Task<HopDong?> GetByIdAsync(ulong id, CancellationToken ct = default);
    Task<ContractDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default);

    Task<PagedResult<ContractDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? search, string? trangThai,
        ulong? khachHangId, CancellationToken ct = default);

    Task<HopDong> AddAsync(HopDong contract, CancellationToken ct = default);
    Task UpdateStatusAsync(ulong id, string trangThai, CancellationToken ct = default);
    Task<bool> DeleteAsync(ulong id, CancellationToken ct = default);

    Task<string> GenerateMaHopDongAsync(CancellationToken ct = default);

    //Kiểm tra báo giá đã được dùng để tạo hợp đồng khác chưa (tránh tạo 2 hợp
    /// đồng từ cùng 1 báo giá
    Task<bool> ExistsForBaoGiaAsync(ulong baoGiaId, CancellationToken ct = default);
}
