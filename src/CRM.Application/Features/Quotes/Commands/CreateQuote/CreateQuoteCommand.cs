using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Quotes.DTOs;
using CRM.Application.Features.Quotes.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Products;
using CRM.Application.Interfaces.Quotes;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Quotes.Commands.CreateQuote;

public record CreateQuoteCommand(
    ulong KhachHangId,
    List<QuoteItemRequestDto> ChiTiet
) : IRequest<QuoteDetailDto>;

public class CreateQuoteCommandValidator : AbstractValidator<CreateQuoteCommand>
{
    public CreateQuoteCommandValidator()
    {
        RuleFor(x => x.KhachHangId).GreaterThan(0UL).WithMessage("Khách hàng không hợp lệ.");

        RuleFor(x => x.ChiTiet)
            .NotEmpty().WithMessage("Báo giá phải có ít nhất 1 sản phẩm.");

        RuleForEach(x => x.ChiTiet).ChildRules(item =>
        {
            item.RuleFor(i => i.SanPhamId).GreaterThan(0U).WithMessage("Sản phẩm không hợp lệ.");
            item.RuleFor(i => i.SoLuong).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");
            item.RuleFor(i => i.DonGia).GreaterThanOrEqualTo(0)
                .When(i => i.DonGia.HasValue)
                .WithMessage("Đơn giá không được âm.");
        });
    }
}

public class CreateQuoteCommandHandler : IRequestHandler<CreateQuoteCommand, QuoteDetailDto>
{
    private const string AuditTable = "HD_BaoGia";
    private readonly IQuoteRepository _quoteRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateQuoteCommandHandler> _logger;

    public CreateQuoteCommandHandler(
        IQuoteRepository quoteRepository, ICustomerRepository customerRepository,
        IProductRepository productRepository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ICurrentUserService currentUser,
        ILogger<CreateQuoteCommandHandler> logger)
    {
        _quoteRepository = quoteRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<QuoteDetailDto> Handle(CreateQuoteCommand request, CancellationToken ct)
    {
        var khachHang = await _customerRepository.GetByIdAsync(request.KhachHangId, ct)
            ?? throw new NotFoundException(nameof(CRM.Domain.Entities.Customers.KhachHang), request.KhachHangId);

        // Sale chỉ lập báo giá cho khách hàng mình phụ trách.
        if (_currentUser.Role == Roles.Sale && khachHang.NhanVienPhuTrachId != _currentUser.UserId)
            throw new ForbiddenException("Bạn chỉ có thể lập báo giá cho khách hàng mình phụ trách.");

        var chiTietInputs = new List<BaoGiaChiTietInput>();
        decimal tongTien = 0;

        foreach (var item in request.ChiTiet)
        {
            var product = await _productRepository.GetByIdAsync(item.SanPhamId, ct)
                ?? throw new NotFoundException(nameof(CRM.Domain.Entities.Products.SanPham), item.SanPhamId);

            if (!product.DangKinhDoanh)
                throw new BusinessRuleException($"Sản phẩm '{product.TenSP}' đã ngừng kinh doanh, không thể đưa vào báo giá.");

            var donGia = item.DonGia ?? product.GiaBan ?? 0m;
            chiTietInputs.Add(new BaoGiaChiTietInput(item.SanPhamId, item.SoLuong, donGia));
            tongTien += item.SoLuong * donGia;
        }

        var maBaoGia = await _quoteRepository.GenerateMaBaoGiaAsync(ct);

        var quote = new BaoGia
        {
            MaBaoGia = maBaoGia,
            KhachHangId = request.KhachHangId,
            TongTien = tongTien,
            TrangThai = QuoteStatus.Nhap,
            NhanVienId = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _quoteRepository.AddAsync(quote, chiTietInputs, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = await _quoteRepository.GetByIdEnrichedAsync(created.Id, ct)
            ?? throw new BusinessRuleException("Tạo báo giá thất bại.");

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                oldData: null, newData: dto, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for quote {Id}", created.Id); }

        return dto;
    }
}