using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Products;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Products.Commands.SetMainProductImage;

public record SetMainProductImageCommand(uint SanPhamId, ulong ImageId) : IRequest<bool>;

public class SetMainProductImageCommandValidator : AbstractValidator<SetMainProductImageCommand>
{
    public SetMainProductImageCommandValidator()
    {
        RuleFor(x => x.SanPhamId).GreaterThan(0U);
        RuleFor(x => x.ImageId).GreaterThan(0UL);
    }
}

public class SetMainProductImageCommandHandler : IRequestHandler<SetMainProductImageCommand, bool>
{
    private readonly IProductRepository _productRepository;
    public SetMainProductImageCommandHandler(IProductRepository productRepository) => _productRepository = productRepository;

    public async Task<bool> Handle(SetMainProductImageCommand request, CancellationToken ct)
    {
        var ok = await _productRepository.SetMainImageAsync(request.SanPhamId, request.ImageId, ct);
        if (!ok) throw new NotFoundException(nameof(SanPhamHinhAnh), request.ImageId);
        return true;
    }
}
