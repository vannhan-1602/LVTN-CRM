using CRM.Application.Features.Products.DTOs;
using CRM.Application.Interfaces.Products;
using MediatR;

namespace CRM.Application.Features.Products.Queries.GetProductImageById;

// Trả về null nếu không tìm thấy (không throw NotFound) vì DeleteImage controller
// dùng kết quả này chỉ để lấy đường dẫn file vật lý trước khi xóa, không phải endpoint đọc chính.
public record GetProductImageByIdQuery(ulong ImageId) : IRequest<ProductImageDto?>;

public class GetProductImageByIdQueryHandler : IRequestHandler<GetProductImageByIdQuery, ProductImageDto?>
{
    private readonly IProductRepository _productRepository;
    public GetProductImageByIdQueryHandler(IProductRepository productRepository) => _productRepository = productRepository;

    public async Task<ProductImageDto?> Handle(GetProductImageByIdQuery request, CancellationToken ct) =>
        await _productRepository.GetImageByIdAsync(request.ImageId, ct);
}
