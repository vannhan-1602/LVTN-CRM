using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Interfaces.Tickets;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Tickets.Commands.SubmitCsat;

// Khách gửi đánh giá hài lòng qua link public (không cần đăng nhập), xác thực bằng token
// lưu trong TK_DanhGiaHaiLong — tương tự cơ chế QuotePublicController.
public record SubmitCsatCommand(string Token, byte DiemDanhGia, string? NhanXet) : IRequest<CsatDto>;

public class SubmitCsatCommandValidator : AbstractValidator<SubmitCsatCommand>
{
    public SubmitCsatCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.DiemDanhGia).InclusiveBetween((byte)1, (byte)5)
            .WithMessage("Điểm đánh giá phải từ 1 đến 5.");
        RuleFor(x => x.NhanXet).MaximumLength(2000);
    }
}

public class SubmitCsatCommandHandler : IRequestHandler<SubmitCsatCommand, CsatDto>
{
    private readonly ICsatRepository _csatRepository;
    public SubmitCsatCommandHandler(ICsatRepository csatRepository) => _csatRepository = csatRepository;

    public async Task<CsatDto> Handle(SubmitCsatCommand request, CancellationToken ct)
    {
        var existing = await _csatRepository.GetByTokenAsync(request.Token, ct)
            ?? throw new NotFoundException("Liên kết đánh giá không tồn tại hoặc đã hết hạn.");

        if (existing.DaDanhGia)
            throw new BusinessRuleException("Ticket này đã được đánh giá trước đó.");

        var result = await _csatRepository.SubmitAsync(request.Token, request.DiemDanhGia, request.NhanXet, ct)
            ?? throw new BusinessRuleException("Gửi đánh giá thất bại, vui lòng thử lại.");

        return result;
    }
}
