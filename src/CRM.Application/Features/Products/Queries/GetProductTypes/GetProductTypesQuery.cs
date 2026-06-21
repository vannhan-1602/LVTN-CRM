using CRM.Application.Features.Products.DTOs;
using CRM.Application.Features.Products.Mappings;
using CRM.Application.Interfaces.Products;
using MediatR;

namespace CRM.Application.Features.Products.Queries.GetProductTypes;

public record GetProductTypesQuery : IRequest<List<ProductTypeDto>>;

public class GetProductTypesQueryHandler : IRequestHandler<GetProductTypesQuery, List<ProductTypeDto>>
{
    private readonly IProductRepository _productRepository;
    public GetProductTypesQueryHandler(IProductRepository productRepository) => _productRepository = productRepository;

    public async Task<List<ProductTypeDto>> Handle(GetProductTypesQuery request, CancellationToken ct)
    {
        var types = await _productRepository.GetAllLoaiSanPhamAsync(ct);
        return types.Select(ProductMapper.ToDto).ToList();
    }
}
