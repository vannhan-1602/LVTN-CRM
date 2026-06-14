using CRM.Application.Common.Models;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Features.Tickets.Mappings;
using CRM.Application.Interfaces.Tickets;
using MediatR;

namespace CRM.Application.Features.Tickets.Queries.GetAllTickets
{
    public class GetAllTicketsQueryHandler : IRequestHandler<GetAllTicketsQuery, PagedResult<TicketDto>>
    {
        private readonly ITicketRepository _ticketRepository;
        public GetAllTicketsQueryHandler(ITicketRepository ticketRepository)
            => _ticketRepository = ticketRepository;

        public async Task<PagedResult<TicketDto>> Handle(GetAllTicketsQuery request, CancellationToken cancellationToken)
        {
            var result = await _ticketRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.Search,
                request.TrangThai,
                request.MucDoUuTien,
                request.KhachHangId,
                request.NhanVienXuLyId,
                cancellationToken);

            return new PagedResult<TicketDto>
            {
                Items = result.Items.Select(TicketMapper.ToDto).ToList(),
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }
    }
}