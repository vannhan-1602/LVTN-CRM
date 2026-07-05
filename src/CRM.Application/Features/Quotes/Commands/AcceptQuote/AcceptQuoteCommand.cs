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

namespace CRM.Application.Features.Quotes.Commands.AcceptQuote;


/// Đánh dấu báo giá được khách hàng chấp nhận. Đây CHƯA tạo hợp đồng — việc tạo
/// hợp đồng là action riêng (CreateContractFromQuote) để Sale/Manager xác nhận
/// thêm thông tin (ngày ký, thời hạn) trước khi sinh hợp đồng chính thức.


public record AcceptQuoteCommand(ulong Id) : IRequest<QuoteDto>;

public class AcceptQuoteCommandValidator : AbstractValidator<AcceptQuoteCommand>
{
    public AcceptQuoteCommandValidator() => RuleFor(x => x.Id).GreaterThan(0UL);
}

public class AcceptQuoteCommandHandler : IRequestHandler<AcceptQuoteCommand, QuoteDto>
{
    private const string AuditTable = "HD_BaoGia";
    private readonly IQuoteRepository _quoteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AcceptQuoteCommandHandler> _logger;

    public AcceptQuoteCommandHandler(
        IQuoteRepository quoteRepository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ICurrentUserService currentUser,
        ILogger<AcceptQuoteCommandHandler> logger)
    {
        _quoteRepository = quoteRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<QuoteDto> Handle(AcceptQuoteCommand request, CancellationToken ct)
    {
        var quote = await _quoteRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);

        if (_currentUser.Role == Roles.Sale && quote.NhanVienId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền thao tác trên báo giá của nhân viên khác.");

        if (quote.TrangThai != QuoteStatus.DaGui)
            throw new BusinessRuleException("Chỉ có thể chấp nhận báo giá đang ở trạng thái Đã gửi.");

        await _quoteRepository.UpdateStatusAsync(request.Id, QuoteStatus.ChapNhan, ct: ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = await _quoteRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "UPDATE",
                oldData: new { TrangThai = QuoteStatus.DaGui }, newData: new { TrangThai = QuoteStatus.ChapNhan }, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for quote {Id}", request.Id); }

        return dto;
    }
}
