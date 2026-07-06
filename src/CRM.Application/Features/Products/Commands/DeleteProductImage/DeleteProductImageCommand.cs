using CRM.Application.Interfaces.Products;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Products.Commands.DeleteProductImage;

// Xóa cứng bản ghi ảnh (khác với sản phẩm chỉ khóa kinh doanh) vì ảnh không có
// ràng buộc nghiệp vụ nào khác tham chiếu tới; file vật lý được xóa ở ProductController
// sau khi command này trả về true.
public record DeleteProductImageCommand(ulong ImageId) : IRequest<bool>;

public class DeleteProductImageCommandValidator : AbstractValidator<DeleteProductImageCommand>
{
    public DeleteProductImageCommandValidator() => RuleFor(x => x.ImageId).GreaterThan(0UL);
}

public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, bool>
{
    private readonly IProductRepository _productRepository;
    public DeleteProductImageCommandHandler(IProductRepository productRepository) => _productRepository = productRepository;

    public async Task<bool> Handle(DeleteProductImageCommand request, CancellationToken ct) =>
        await _productRepository.DeleteImageAsync(request.ImageId, ct);
}
