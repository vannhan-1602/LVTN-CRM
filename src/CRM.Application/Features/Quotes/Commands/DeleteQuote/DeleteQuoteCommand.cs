using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Quotes;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Quotes.Commands.DeleteQuote;

public record DeleteQuoteCommand(ulong Id) : IRequest<bool>;

public class DeleteQuoteCommandValidator : AbstractValidator<DeleteQuoteCommand>
{
    public DeleteQuoteCommandValidator() => RuleFor(x => x.Id).GreaterThan(0UL);
}

public class DeleteQuoteCommandHandler : IRequestHandler<DeleteQuoteCommand, bool>
{
    private const string AuditTable = "HD_BaoGia";
    private readonly IQuoteRepository _quoteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DeleteQuoteCommandHandler> _logger;

    public DeleteQuoteCommandHandler(
        IQuoteRepository quoteRepository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ICurrentUserService currentUser,
        ILogger<DeleteQuoteCommandHandler> logger)
    {
        _quoteRepository = quoteRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteQuoteCommand request, CancellationToken ct)
    {
        var quote = await _quoteRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);

        if (_currentUser.Role == Roles.Sale && quote.NhanVienId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền xóa báo giá của nhân viên khác.");

        if (quote.TrangThai != QuoteStatus.Nhap)
            throw new BusinessRuleException("Chỉ có thể xóa báo giá đang ở trạng thái Nháp.");

        var deleted = await _quoteRepository.DeleteAsync(request.Id, ct);
        if (!deleted) throw new NotFoundException(nameof(BaoGia), request.Id);

        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "DELETE",
                oldData: quote, newData: null, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for quote {Id}", request.Id); }

        return true;
    }
}
