using CRM.Application.Features.Products.DTOs;
using CRM.Application.Interfaces.Products;
using MediatR;

namespace CRM.Application.Features.Products.Queries.GetProductImages;

public record GetProductImagesQuery(uint SanPhamId) : IRequest<List<ProductImageDto>>;

public class GetProductImagesQueryHandler : IRequestHandler<GetProductImagesQuery, List<ProductImageDto>>
{
    private readonly IProductRepository _productRepository;
    public GetProductImagesQueryHandler(IProductRepository productRepository) => _productRepository = productRepository;

    public async Task<List<ProductImageDto>> Handle(GetProductImagesQuery request, CancellationToken ct) =>
        await _productRepository.GetImagesAsync(request.SanPhamId, ct);
}
