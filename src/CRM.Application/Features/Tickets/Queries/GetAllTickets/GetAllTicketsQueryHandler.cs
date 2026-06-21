using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Features.Tickets.Mappings;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Tickets;
using MediatR;

namespace CRM.Application.Features.Tickets.Queries.GetAllTickets
{
    public class GetAllTicketsQueryHandler : IRequestHandler<GetAllTicketsQuery, PagedResult<TicketDto>>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ICurrentUserService _currentUser;

        public GetAllTicketsQueryHandler(ITicketRepository ticketRepository, ICurrentUserService currentUser)
        {
            _ticketRepository = ticketRepository;
            _currentUser = currentUser;
        }

        public async Task<PagedResult<TicketDto>> Handle(GetAllTicketsQuery request, CancellationToken cancellationToken)
        {
            //  Sale chỉ xem Ticket mình xử lý
            var nhanVienXuLyId = _currentUser.Role == Roles.Sale
                ? _currentUser.NhanSuId
                : request.NhanVienXuLyId;

            var result = await _ticketRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.Search,
                request.TrangThai,
                request.MucDoUuTien,
                request.KhachHangId,
                nhanVienXuLyId,
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
