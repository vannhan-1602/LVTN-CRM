using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Products.Commands.CreateProduct;
using CRM.Application.Features.Products.Commands.DeleteProduct;
using CRM.Application.Features.Products.Commands.UpdateProduct;
using CRM.Application.Features.Products.Commands.UpdateStock;
using CRM.Application.Features.Products.DTOs;
using CRM.Application.Features.Products.Queries.GetAllProducts;
using CRM.Application.Features.Products.Queries.GetProductById;
using CRM.Application.Features.Products.Queries.GetProductTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

// Quản lý Sản phẩm/Dịch vụ và Kho. Sản phẩm là danh mục dùng chung cho Báo giá,
// nên không thuộc phạm vi "dữ liệu riêng của Sale" — mọi thành viên SalesTeam đều xem/lập báo giá được từ danh mục chung; chỉ Manager mới thêm/sửa/khóa sản phẩm
// điều chỉnh kho (nghiệp vụ quản lý danh mục, không phải nghiệp vụ bán hàng cá nhân).

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductController(IMediator mediator) => _mediator = mediator;

    // Đọc: toàn bộ SalesTeam (Sale cần xem danh mục để lập báo giá)
    [HttpGet]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] uint? loaiSanPhamId = null,
        [FromQuery] bool? dangKinhDoanh = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAllProductsQuery(pageNumber, pageSize, search, loaiSanPhamId, dangKinhDoanh), ct);
        return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> GetById(uint id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), ct);
        return Ok(ApiResponse<ProductDetailDto>.Ok(result));
    }

    [HttpGet("types")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> GetTypes(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductTypesQuery(), ct);
        return Ok(ApiResponse<List<ProductTypeDto>>.Ok(result));
    }

    // Ghi: chỉ Manager (quản lý danh mục sản phẩm là nghiệp vụ cấp quản lý)
    [HttpPost]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateProductCommand(
            request.LoaiSanPhamId, request.MaSP, request.TenSP,
            request.DonVi, request.GiaBan, request.SoLuongTonBanDau), ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ProductDto>.Ok(result, "Tạo sản phẩm thành công."));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> Update(uint id, [FromBody] UpdateProductRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProductCommand(
            id, request.LoaiSanPhamId, request.TenSP, request.DonVi,
            request.GiaBan, request.DangKinhDoanh), ct);

        return Ok(ApiResponse<ProductDto>.Ok(result, "Cập nhật sản phẩm thành công."));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> Delete(uint id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductCommand(id), ct);
        return Ok(ApiResponse.Ok("Đã khóa kinh doanh sản phẩm."));
    }

    // Điều chỉnh kho: Manager (nghiệp vụ kho thuộc quyền quản lý, không phải Sale)
    [HttpPost("{id:long}/stock")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> AdjustStock(uint id, [FromBody] AdjustStockRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateStockCommand(
            id, request.LoaiGiaoDich, request.SoLuong, request.MaChungTu, request.GhiChu), ct);

        return Ok(ApiResponse<StockTransactionResultDto>.Ok(result, "Cập nhật tồn kho thành công."));
    }
}
