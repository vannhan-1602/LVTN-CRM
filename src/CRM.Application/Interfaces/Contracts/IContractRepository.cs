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

    /// <summary>Khách hàng còn hợp đồng đang hiệu lực (DangThucHien) hay không — dùng để chặn xóa KH.</summary>
    Task<bool> HasActiveContractAsync(ulong khachHangId, CancellationToken ct = default);

    /// <summary>Tạo các dòng lịch trả góp (HD_LichThanhToan) cho 1 hợp đồng trả góp.</summary>
    Task AddLichThanhToanRangeAsync(
        ulong hopDongId,
        IEnumerable<(int SoDot, decimal SoTien, DateOnly HanThanhToan)> items,
        CancellationToken ct = default);

    /// <summary>Lấy toàn bộ lịch trả góp của 1 hợp đồng, sắp theo SoDot tăng dần.</summary>
    Task<List<LichThanhToanDto>> GetLichThanhToanByHopDongAsync(ulong hopDongId, CancellationToken ct = default);
}