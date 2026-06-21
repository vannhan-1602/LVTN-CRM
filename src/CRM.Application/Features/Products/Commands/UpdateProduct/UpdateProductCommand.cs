using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Products.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Products;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    uint Id,
    uint? LoaiSanPhamId,
    string TenSP,
    string? DonVi,
    decimal GiaBan,
    bool DangKinhDoanh
) : IRequest<ProductDto>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0U);
        RuleFor(x => x.TenSP).NotEmpty().WithMessage("Tên sản phẩm không được để trống.").MaximumLength(255);
        RuleFor(x => x.GiaBan).GreaterThanOrEqualTo(0).WithMessage("Giá bán không được âm.");
        RuleFor(x => x.DonVi).MaximumLength(50);
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private const string AuditTable = "BH_SanPham";
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(SanPham), request.Id);

        if (request.LoaiSanPhamId.HasValue &&
            !await _productRepository.LoaiSanPhamExistsAsync(request.LoaiSanPhamId.Value, ct))
            throw new BusinessRuleException("Loại sản phẩm không hợp lệ.");

        var oldDto = await _productRepository.GetByIdEnrichedAsync(request.Id, ct);

        product.LoaiSanPhamId = request.LoaiSanPhamId;
        product.TenSP = request.TenSP.Trim();
        product.DonVi = request.DonVi?.Trim();
        product.GiaBan = request.GiaBan;
        product.DangKinhDoanh = request.DangKinhDoanh;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var newDto = await _productRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(SanPham), request.Id);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "UPDATE",
                oldData: oldDto, newData: newDto, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for product {Id}", request.Id); }

        return newDto;
    }
}
