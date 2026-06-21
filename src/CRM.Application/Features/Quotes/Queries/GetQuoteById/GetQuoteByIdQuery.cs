using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Quotes.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Quotes;
using CRM.Domain.Entities.Sales;
using MediatR;

namespace CRM.Application.Features.Quotes.Queries.GetQuoteById;

public record GetQuoteByIdQuery(ulong Id) : IRequest<QuoteDetailDto>;

public class GetQuoteByIdQueryHandler : IRequestHandler<GetQuoteByIdQuery, QuoteDetailDto>
{
    private readonly IQuoteRepository _quoteRepository;
    private readonly ICurrentUserService _currentUser;

    public GetQuoteByIdQueryHandler(IQuoteRepository quoteRepository, ICurrentUserService currentUser)
    {
        _quoteRepository = quoteRepository;
        _currentUser = currentUser;
    }

    public async Task<QuoteDetailDto> Handle(GetQuoteByIdQuery request, CancellationToken ct)
    {
        var quote = await _quoteRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);

        if (_currentUser.Role == Roles.Sale && quote.NhanVienId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền xem báo giá của nhân viên khác.");

        return await _quoteRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);
    }
}
