using CRM.Application.Common.Models;
using CRM.Application.Features.Products.DTOs;
using CRM.Application.Interfaces.Products;
using MediatR;

namespace CRM.Application.Features.Products.Queries.GetAllProducts;

public record GetAllProductsQuery(
    int PageNumber,
    int PageSize,
    string? Search,
    uint? LoaiSanPhamId,
    bool? DangKinhDoanh
) : IRequest<PagedResult<ProductDto>>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    public GetAllProductsQueryHandler(IProductRepository productRepository) => _productRepository = productRepository;

    public Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken ct) =>
        _productRepository.GetPagedAsync(
            request.PageNumber, request.PageSize, request.Search,
            request.LoaiSanPhamId, request.DangKinhDoanh, ct);
}
