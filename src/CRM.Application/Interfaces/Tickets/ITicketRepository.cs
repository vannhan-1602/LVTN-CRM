using CRM.Application.Common.Models;
using CRM.Domain.Entities.Tickets;

namespace CRM.Application.Interfaces.Tickets
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default);

        Task<PagedResult<Ticket>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            string? trangThai,
            string? mucDoUuTien,
            ulong? khachHangId,
            uint? nhanVienXuLyId,
            CancellationToken cancellationToken = default);

        Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
        Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(ulong id, CancellationToken cancellationToken = default);

        Task<string> GenerateMaTicketAsync(CancellationToken cancellationToken = default);

        Task<List<TicketPhanHoi>> GetPhanHoisAsync(ulong ticketId, CancellationToken cancellationToken = default);
        Task<TicketPhanHoi> AddPhanHoiAsync(TicketPhanHoi phanHoi, CancellationToken cancellationToken = default);

        Task<bool> LoaiTicketExistsAsync(ushort id, CancellationToken cancellationToken = default);
        Task<bool> KhachHangExistsAsync(ulong id, CancellationToken cancellationToken = default);

        /// <summary>Lấy Id của loại ticket theo tên — dùng cho VoucherRedeemController.</summary>
        Task<ushort?> GetLoaiTicketIdByNameAsync(string tenLoai, CancellationToken ct = default);

        /// <summary>
        /// Tạo ticket tự động khi khách bấm link voucher trong email.
        /// Trả về Id của ticket vừa tạo.
        /// </summary>
        Task<ulong> CreateTicketForVoucherAsync(
            ulong khachHangId, ushort? loaiTicketId,
            string tieuDe, string moTa,
            CancellationToken ct = default);

        /// <summary>Lấy số giờ tối đa để xử lý xong (TK_SLA.SoGioXuLy) theo mức độ ưu tiên — dùng tính ThoiHanSLA khi tạo ticket.</summary>
        Task<int?> GetSlaSoGioXuLyAsync(string mucDoUuTien, CancellationToken ct = default);
    }
}