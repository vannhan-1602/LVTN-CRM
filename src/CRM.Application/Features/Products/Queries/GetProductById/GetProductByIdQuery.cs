using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Products.DTOs;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Products;
using MediatR;

namespace CRM.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(uint Id) : IRequest<ProductDetailDto>;

public class ProductDetailDto : ProductDto
{
    public List<StockTransactionDto> LichSuKho { get; set; } = [];
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    private readonly IProductRepository _productRepository;
    public GetProductByIdQueryHandler(IProductRepository productRepository) => _productRepository = productRepository;

    public async Task<ProductDetailDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var dto = await _productRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(SanPham), request.Id);

        var history = await _productRepository.GetStockHistoryAsync(request.Id, ct);

        return new ProductDetailDto
        {
            Id = dto.Id,
            LoaiSanPhamId = dto.LoaiSanPhamId,
            TenLoai = dto.TenLoai,
            MaSP = dto.MaSP,
            TenSP = dto.TenSP,
            DonVi = dto.DonVi,
            GiaBan = dto.GiaBan,
            SoLuongTon = dto.SoLuongTon,
            DangKinhDoanh = dto.DangKinhDoanh,
            AnhDaiDien = dto.AnhDaiDien,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            LichSuKho = history.OrderByDescending(h => h.NgayGiaoDich).ToList()
        };
    }
}
