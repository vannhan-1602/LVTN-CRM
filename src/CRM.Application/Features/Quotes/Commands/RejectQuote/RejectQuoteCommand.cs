using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Quotes.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Quotes;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Quotes.Commands.RejectQuote;

public record RejectQuoteCommand(ulong Id, string? LyDo) : IRequest<QuoteDto>;

public class RejectQuoteCommandValidator : AbstractValidator<RejectQuoteCommand>
{
    public RejectQuoteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
        RuleFor(x => x.LyDo).MaximumLength(500);
    }
}

public class RejectQuoteCommandHandler : IRequestHandler<RejectQuoteCommand, QuoteDto>
{
    private const string AuditTable = "HD_BaoGia";
    private readonly IQuoteRepository _quoteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RejectQuoteCommandHandler> _logger;

    public RejectQuoteCommandHandler(
        IQuoteRepository quoteRepository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ICurrentUserService currentUser,
        ILogger<RejectQuoteCommandHandler> logger)
    {
        _quoteRepository = quoteRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<QuoteDto> Handle(RejectQuoteCommand request, CancellationToken ct)
    {
        var quote = await _quoteRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);

        if (_currentUser.Role == Roles.Sale && quote.NhanVienId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền thao tác trên báo giá của nhân viên khác.");

        if (quote.TrangThai != QuoteStatus.DaGui)
            throw new BusinessRuleException("Chỉ có thể từ chối báo giá đang ở trạng thái Đã gửi.");

        await _quoteRepository.UpdateStatusAsync(request.Id, QuoteStatus.TuChoi, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = await _quoteRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "UPDATE",
                oldData: new { TrangThai = QuoteStatus.DaGui },
                newData: new { TrangThai = QuoteStatus.TuChoi, LyDo = request.LyDo }, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for quote {Id}", request.Id); }

        return dto;
    }
}
