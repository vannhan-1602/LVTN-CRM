using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Interfaces.Tickets;
using MediatR;

namespace CRM.Application.Features.Tickets.Queries.GetCsatByToken;

// Trả thông tin ticket để render form đánh giá công khai (không cần đăng nhập).
public record GetCsatByTokenQuery(string Token) : IRequest<CsatDto>;

public class GetCsatByTokenQueryHandler : IRequestHandler<GetCsatByTokenQuery, CsatDto>
{
    private readonly ICsatRepository _csatRepository;
    public GetCsatByTokenQueryHandler(ICsatRepository csatRepository) => _csatRepository = csatRepository;

    public async Task<CsatDto> Handle(GetCsatByTokenQuery request, CancellationToken ct) =>
        await _csatRepository.GetByTokenAsync(request.Token, ct)
            ?? throw new NotFoundException("Liên kết đánh giá không tồn tại hoặc đã hết hạn.");
}
