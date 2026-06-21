using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Products;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Products.Commands.DeleteProduct;


// Xóa sản phẩm  là khóa kinh doanh (DangKinhDoanh = false), KHÔNG xóa cứng. 
//Lý do: BH_SanPham_HinhAnh, HD_BaoGia_ChiTiet, Kho_TheKho đều có FK trỏ tới
// Sản phẩm bị khóa sẽ không xuất hiện khi lập báo giá mới.

public record DeleteProductCommand(uint Id) : IRequest<bool>;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator() => RuleFor(x => x.Id).GreaterThan(0U);
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private const string AuditTable = "BH_SanPham";
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IProductRepository productRepository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ILogger<DeleteProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(SanPham), request.Id);

        var deactivated = await _productRepository.DeactivateAsync(request.Id, ct);
        if (!deactivated) throw new NotFoundException(nameof(SanPham), request.Id);

        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "UPDATE",
                oldData: new { product.DangKinhDoanh }, newData: new { DangKinhDoanh = false }, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for product {Id}", request.Id); }

        return true;
    }
}
