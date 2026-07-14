using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Quotes.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Email;
using CRM.Application.Interfaces.Quotes;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Quotes.Commands.SendQuote;

/// Chuyển báo giá từ Nháp sang Đã gửi — đánh dấu đã gửi cho khách hàng, đồng thời
/// gửi email thật kèm link công khai để khách xem/chấp nhận/từ chối không cần đăng nhập.
public record SendQuoteCommand(ulong Id) : IRequest<QuoteDto>;

public class SendQuoteCommandValidator : AbstractValidator<SendQuoteCommand>
{
    public SendQuoteCommandValidator() => RuleFor(x => x.Id).GreaterThan(0UL);
}

public class SendQuoteCommandHandler : IRequestHandler<SendQuoteCommand, QuoteDto>
{
    private const string AuditTable = "HD_BaoGia";
    private readonly IQuoteRepository _quoteRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;
    private readonly IQuotePublicTokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly ILogger<SendQuoteCommandHandler> _logger;

    public SendQuoteCommandHandler(
        IQuoteRepository quoteRepository, ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork, IAuditLogPublisher auditLogPublisher,
        ICurrentUserService currentUser, IEmailService emailService,
        IQuotePublicTokenService tokenService, IConfiguration config,
        ILogger<SendQuoteCommandHandler> logger)
    {
        _quoteRepository = quoteRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _emailService = emailService;
        _tokenService = tokenService;
        _config = config;
        _logger = logger;
    }

    public async Task<QuoteDto> Handle(SendQuoteCommand request, CancellationToken ct)
    {
        var quote = await _quoteRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);

        if (_currentUser.Role == Roles.Sale && quote.NhanVienId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền thao tác trên báo giá của nhân viên khác.");

        if (quote.TrangThai != QuoteStatus.Nhap)
            throw new BusinessRuleException("Chỉ có thể gửi báo giá đang ở trạng thái Nháp.");

        await _quoteRepository.UpdateStatusAsync(request.Id, QuoteStatus.DaGui, ct: ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = await _quoteRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "UPDATE",
                oldData: new { TrangThai = QuoteStatus.Nhap }, newData: new { TrangThai = QuoteStatus.DaGui }, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for quote {Id}", request.Id); }

        // Gửi email thật kèm link công khai — lỗi gửi mail không được làm rollback
        // việc chuyển trạng thái báo giá (giống pattern của LoyaltyService).
        try
        {
            var customer = await _customerRepository.GetByIdAsync(dto.KhachHangId, ct);
            if (customer?.Email is { Length: > 0 })
            {
                var token = _tokenService.GenerateToken(dto.Id);
                var frontendBaseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
                var quoteLink = $"{frontendBaseUrl}/public/quotes/{token}";

                await _emailService.GuiEmailBaoGiaAsync(
                    dto.KhachHangId, dto.TenKhachHang ?? customer.TenKhachHang, customer.Email,
                    dto.MaBaoGia, dto.TongTien, quoteLink, ct);
            }
            else
            {
                _logger.LogWarning(
                    "Báo giá {Id}: khách hàng {KhId} chưa có email, bỏ qua gửi mail.",
                    request.Id, dto.KhachHangId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gửi email báo giá {Id} thất bại", request.Id);
        }

        return dto;
    }
}