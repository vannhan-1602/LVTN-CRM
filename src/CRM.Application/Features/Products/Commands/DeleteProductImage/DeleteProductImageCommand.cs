using CRM.Application.Interfaces.Products;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Products.Commands.DeleteProductImage;

// Xóa cứng bản ghi ảnh (khác với sản phẩm chỉ khóa kinh doanh) vì ảnh không có
// ràng buộc nghiệp vụ nào khác tham chiếu tới; file vật lý được xóa ở ProductController
// sau khi command này trả về true.
// SanPhamId bắt buộc kèm ImageId để đảm bảo ảnh thực sự thuộc đúng sản phẩm trên route
// {id}/images/{imageId} — tránh xóa nhầm/xóa chéo ảnh của sản phẩm khác.
public record DeleteProductImageCommand(uint SanPhamId, ulong ImageId) : IRequest<bool>;

public class DeleteProductImageCommandValidator : AbstractValidator<DeleteProductImageCommand>
{
    public DeleteProductImageCommandValidator()
    {
        RuleFor(x => x.SanPhamId).GreaterThan(0U);
        RuleFor(x => x.ImageId).GreaterThan(0UL);
    }
}

public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, bool>
{
    private readonly IProductRepository _productRepository;
    public DeleteProductImageCommandHandler(IProductRepository productRepository) => _productRepository = productRepository;

    public async Task<bool> Handle(DeleteProductImageCommand request, CancellationToken ct) =>
        await _productRepository.DeleteImageAsync(request.SanPhamId, request.ImageId, ct);
}