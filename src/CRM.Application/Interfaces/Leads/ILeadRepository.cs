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
        string? tinhTrang = null,
        //  true: chỉ lấy Lead chưa gán (NhanVienPhuTrachId null) — hàng chờ để Sale tự
        // nhận, ưu tiên hơn ownerUserId vì 2 filter này loại trừ nhau về mặt nghiệp vụ.
        bool? chuaGan = null,
        CancellationToken cancellationToken = default);

    /// <summary>Kiểm tra email đã tồn tại ở Lead khác chưa (loại trừ chính lead đang sửa qua excludeId).</summary>
    Task<bool> EmailExistsAsync(string email, ulong? excludeId = null, CancellationToken cancellationToken = default);

    Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default);
    Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(ulong id, CancellationToken cancellationToken = default);
    Task<bool> RestoreAsync(ulong id, CancellationToken cancellationToken = default);

    /// <summary>Gán/tự nhận phụ trách Lead — ATOMIC bằng 1 câu UPDATE có điều kiện ngay trong
    /// WHERE (cùng kỹ thuật đã dùng cho CsatRepository.SubmitAsync / VoucherRepository.RedeemAsync),
    /// KHÔNG phải đọc-rồi-ghi như UpdateAsync ở trên.
    /// - restrictIfCurrentOwnerNot = null: không giới hạn (Manager — luôn gán được).
    /// - restrictIfCurrentOwnerNot có giá trị (Sale tự nhận): chỉ update THÀNH CÔNG nếu
    ///   NhanVienPhuTrach_Id hiện tại trong DB đang NULL hoặc đúng bằng giá trị này — nếu 1 Sale
    ///   khác vừa nhận mất Lead này trong lúc chờ, điều kiện WHERE không khớp, trả về false,
    ///   KHÔNG ghi đè lên người đã nhận trước.</summary>
    Task<bool> TryAssignAsync(ulong id, uint? newOwnerId, uint? restrictIfCurrentOwnerNot, CancellationToken cancellationToken = default);
}