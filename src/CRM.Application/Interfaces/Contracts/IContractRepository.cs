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

    /// <summary>Lấy 1 đợt trả góp theo Id — dùng để validate khi tạo hóa đơn cho đợt trả góp.</summary>
    Task<LichThanhToanDto?> GetLichThanhToanByIdAsync(ulong id, CancellationToken ct = default);

    /// <summary>Đánh dấu 1 đợt trả góp đã thanh toán xong (gọi khi hóa đơn ứng với đợt đó HoanTat).</summary>
    Task MarkLichThanhToanDaThanhToanAsync(ulong lichThanhToanId, CancellationToken ct = default);

    /// <summary>
    /// Khoá dòng hợp đồng (SELECT ... FOR UPDATE) trong transaction hiện tại của request — dùng để
    /// serialize các request tạo hóa đơn đồng thời cho CÙNG 1 hợp đồng. Nếu không khoá, 2 kế toán
    /// bấm "Tạo hóa đơn" cùng lúc có thể cùng đọc được "còn hạn mức" trước khi ai kịp ghi, dẫn tới
    /// tổng tiền vượt giá trị hợp đồng hoặc 2 hóa đơn cùng trỏ vào 1 đợt trả góp. Vì toàn bộ command
    /// đã chạy trong 1 transaction (xem TransactionBehavior), lock sẽ tự giải phóng khi commit/rollback.
    /// </summary>
    Task LockHopDongForUpdateAsync(ulong hopDongId, CancellationToken ct = default);

    /// <summary>Đánh dấu hợp đồng vừa được nhắc gia hạn (chống job tạo Ticket nhắc trùng).</summary>
    Task MarkDaNhacGiaHanAsync(ulong id, DateOnly ngayNhac, CancellationToken ct = default);

    /// <summary>Các hợp đồng gia hạn/bảo trì được tạo ra TỪ hợp đồng gốc — dùng hiển thị badge liên kết 2 chiều.</summary>
    Task<List<ContractRenewalLinkDto>> GetRenewalLinksAsync(ulong hopDongGocId, CancellationToken ct = default);
}