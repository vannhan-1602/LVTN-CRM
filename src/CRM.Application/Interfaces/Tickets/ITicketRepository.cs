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
    }
}