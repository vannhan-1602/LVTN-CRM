using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Features.Tickets.Mappings;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Tickets;
using MediatR;

namespace CRM.Application.Features.Tickets.Queries.GetAllTickets;

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
        // Sale mặc định chỉ xem ticket mình đang xử lý, TRỪ khi:
        //  - request.ChuaGan = true → xem hàng chờ ticket chưa gán (để tự nhận), mọi Sale đều thấy như nhau.
        //  - request.NhanVienXuLyId đã chỉ đích danh chính mình → giữ nguyên (tương đương mặc định).
        // Sale không được xem ticket đang thuộc về đồng nghiệp khác bằng cách truyền nhanVienXuLyId của người khác.
        var chuaGan = request.ChuaGan;
        uint? nhanVienXuLyId;

        if (_currentUser.Role == Roles.Sale)
        {
            nhanVienXuLyId = chuaGan == true ? null : _currentUser.UserId;
        }
        else
        {
            nhanVienXuLyId = request.NhanVienXuLyId;
        }

        var result = await _ticketRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.TrangThai,
            request.MucDoUuTien,
            request.KhachHangId,
            nhanVienXuLyId,
            chuaGan,
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