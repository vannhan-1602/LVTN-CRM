using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Quotes.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Quotes;
using MediatR;

namespace CRM.Application.Features.Quotes.Queries.GetAllQuotes;

public record GetAllQuotesQuery(
    int PageNumber, int PageSize, string? Search, string? TrangThai, ulong? KhachHangId
) : IRequest<PagedResult<QuoteDto>>;

public class GetAllQuotesQueryHandler : IRequestHandler<GetAllQuotesQuery, PagedResult<QuoteDto>>
{
    private readonly IQuoteRepository _quoteRepository;
    private readonly ICurrentUserService _currentUser;

    public GetAllQuotesQueryHandler(IQuoteRepository quoteRepository, ICurrentUserService currentUser)
    {
        _quoteRepository = quoteRepository;
        _currentUser = currentUser;
    }

    public Task<PagedResult<QuoteDto>> Handle(GetAllQuotesQuery request, CancellationToken ct)
    {
        //  Sale chỉ xem báo giá do chính mình lập
        uint? ownerUserId = _currentUser.Role == Roles.Sale ? _currentUser.UserId : null;

        return _quoteRepository.GetPagedAsync(
            request.PageNumber, request.PageSize, request.Search,
            request.TrangThai, request.KhachHangId, ownerUserId, ct);
    }
}
