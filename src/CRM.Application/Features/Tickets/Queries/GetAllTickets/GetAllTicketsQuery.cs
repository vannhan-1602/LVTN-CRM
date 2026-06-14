using CRM.Application.Common.Models;
using CRM.Application.Features.Tickets.DTOs;
using MediatR;

namespace CRM.Application.Features.Tickets.Queries.GetAllTickets
{
    public record GetAllTicketsQuery(
        int PageNumber,
        int PageSize,
        string? Search,
        string? TrangThai,
        string? MucDoUuTien,
        ulong? KhachHangId,
        uint? NhanVienXuLyId) : IRequest<PagedResult<TicketDto>>;
}