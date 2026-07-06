using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Products.DTOs;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Products;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Products.Commands.AddProductImage;

public record AddProductImageCommand(uint SanPhamId, string UrlHinhAnh, bool IsMain) : IRequest<ProductImageDto>;

public class AddProductImageCommandValidator : AbstractValidator<AddProductImageCommand>
{
    public AddProductImageCommandValidator()
    {
        RuleFor(x => x.SanPhamId).GreaterThan(0U);
        RuleFor(x => x.UrlHinhAnh).NotEmpty().MaximumLength(500);
    }
}

public class AddProductImageCommandHandler : IRequestHandler<AddProductImageCommand, ProductImageDto>
{
    private readonly IProductRepository _productRepository;
    public AddProductImageCommandHandler(IProductRepository productRepository) => _productRepository = productRepository;

    public async Task<ProductImageDto> Handle(AddProductImageCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.SanPhamId, ct)
            ?? throw new NotFoundException(nameof(SanPham), request.SanPhamId);

        return await _productRepository.AddImageAsync(request.SanPhamId, request.UrlHinhAnh, request.IsMain, ct);
    }
}
