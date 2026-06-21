using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Quotes.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Products;
using CRM.Application.Interfaces.Quotes;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Quotes.Commands.UpdateQuote;

public record UpdateQuoteCommand(ulong Id, List<QuoteItemRequestDto> ChiTiet) : IRequest<QuoteDetailDto>;

public class UpdateQuoteCommandValidator : AbstractValidator<UpdateQuoteCommand>
{
    public UpdateQuoteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
        RuleFor(x => x.ChiTiet).NotEmpty().WithMessage("Báo giá phải có ít nhất 1 sản phẩm.");

        RuleForEach(x => x.ChiTiet).ChildRules(item =>
        {
            item.RuleFor(i => i.SanPhamId).GreaterThan(0U).WithMessage("Sản phẩm không hợp lệ.");
            item.RuleFor(i => i.SoLuong).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");
        });
    }
}

public class UpdateQuoteCommandHandler : IRequestHandler<UpdateQuoteCommand, QuoteDetailDto>
{
    private const string AuditTable = "HD_BaoGia";
    private readonly IQuoteRepository _quoteRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateQuoteCommandHandler> _logger;

    public UpdateQuoteCommandHandler(
        IQuoteRepository quoteRepository, IProductRepository productRepository,
        IUnitOfWork unitOfWork, IAuditLogPublisher auditLogPublisher,
        ICurrentUserService currentUser, ILogger<UpdateQuoteCommandHandler> logger)
    {
        _quoteRepository = quoteRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<QuoteDetailDto> Handle(UpdateQuoteCommand request, CancellationToken ct)
    {
        var quote = await _quoteRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);

        if (_currentUser.Role == Roles.Sale && quote.NhanVienId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền sửa báo giá của nhân viên khác.");

        if (quote.TrangThai != QuoteStatus.Nhap)
            throw new BusinessRuleException("Chỉ có thể sửa báo giá đang ở trạng thái Nháp.");

        var oldDto = await _quoteRepository.GetByIdEnrichedAsync(request.Id, ct);

        var chiTietInputs = new List<Interfaces.Quotes.BaoGiaChiTietInput>();
        decimal tongTien = 0;

        foreach (var item in request.ChiTiet)
        {
            var product = await _productRepository.GetByIdAsync(item.SanPhamId, ct)
                ?? throw new NotFoundException(nameof(CRM.Domain.Entities.Products.SanPham), item.SanPhamId);

            if (!product.DangKinhDoanh)
                throw new BusinessRuleException($"Sản phẩm '{product.TenSP}' đã ngừng kinh doanh.");

            var donGia = item.DonGia ?? product.GiaBan;
            chiTietInputs.Add(new Interfaces.Quotes.BaoGiaChiTietInput(item.SanPhamId, item.SoLuong, donGia));
            tongTien += item.SoLuong * donGia;
        }

        quote.TongTien = tongTien;
        quote.UpdatedAt = DateTime.UtcNow;

        await _quoteRepository.UpdateAsync(quote, chiTietInputs, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var newDto = await _quoteRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.Id);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "UPDATE",
                oldData: oldDto, newData: newDto, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for quote {Id}", request.Id); }

        return newDto;
    }
}
