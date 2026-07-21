using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Products.Commands.AddProductImage;
using CRM.Application.Features.Products.Commands.CreateProduct;
using CRM.Application.Features.Products.Commands.DeleteProduct;
using CRM.Application.Features.Products.Commands.DeleteProductImage;
using CRM.Application.Features.Products.Commands.SetMainProductImage;
using CRM.Application.Features.Products.Commands.UpdateProduct;
using CRM.Application.Features.Products.Commands.UpdateStock;
using CRM.Application.Features.Products.DTOs;
using CRM.Application.Features.Products.Queries.GetAllProducts;
using CRM.Application.Features.Products.Queries.GetProductById;
using CRM.Application.Features.Products.Queries.GetProductImageById;
using CRM.Application.Features.Products.Queries.GetProductImages;
using CRM.Application.Features.Products.Queries.GetProductTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace CRM.API.Controllers;

// Quản lý Sản phẩm/Dịch vụ và Kho.
// chỉ Manager mới thêm/sửa/khóa sản phẩm điều chỉnh kho (nghiệp vụ quản lý danh mục, không phải nghiệp vụ bán hàng cá nhân).

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;
    public ProductController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

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

    //Quản lý hình ảnh sản phẩm (1 ảnh chính + nhiều ảnh phụ)

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB

    [HttpGet("{id:long}/images")]
    [Authorize(Policy = Policies.SalesTeam)]
    [ProducesResponseType(typeof(ApiResponse<List<ProductImageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImages(uint id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductImagesQuery(id), ct);
        return Ok(ApiResponse<List<ProductImageDto>>.Ok(result));
    }

    // Upload ảnh mới cho sản phẩm (multipart/form-data). isMain=true để đặt làm ảnh đại diện
    // ngay khi upload; nếu sản phẩm chưa có ảnh chính nào thì ảnh đầu tiên tự động là ảnh chính.
    [HttpPost("{id:long}/images")]
    [Authorize(Policy = Policies.ManagerOnly)]
    [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(MaxImageSizeBytes)]
    public async Task<IActionResult> UploadImage(uint id, IFormFile file, [FromForm] bool isMain, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("Vui lòng chọn file ảnh."));

        if (file.Length > MaxImageSizeBytes)
            return BadRequest(ApiResponse.Fail("Ảnh vượt quá dung lượng cho phép (tối đa 5MB)."));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            return BadRequest(ApiResponse.Fail("Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .webp, .gif)."));

        var fileName = $"{Guid.NewGuid()}{ext}";
        var uploadsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "products");
        Directory.CreateDirectory(uploadsDir);
        var fullPath = Path.Combine(uploadsDir, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var url = $"/uploads/products/{fileName}";

        try
        {
            var result = await _mediator.Send(new AddProductImageCommand(id, url, isMain), ct);
            return CreatedAtAction(nameof(GetImages), new { id }, ApiResponse<ProductImageDto>.Ok(result, "Tải ảnh lên thành công."));
        }
        catch
        {
            // Rollback file vật lý nếu command thất bại (vd: sản phẩm không tồn tại)
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            throw;
        }
    }

    [HttpPut("{id:long}/images/{imageId:long}/set-main")]
    [Authorize(Policy = Policies.ManagerOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetMainImage(uint id, ulong imageId, CancellationToken ct)
    {
        await _mediator.Send(new SetMainProductImageCommand(id, imageId), ct);
        return Ok(ApiResponse.Ok("Đã đặt làm ảnh đại diện."));
    }

    [HttpDelete("{id:long}/images/{imageId:long}")]
    [Authorize(Policy = Policies.ManagerOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteImage(uint id, ulong imageId, CancellationToken ct)
    {
        // Lấy thông tin ảnh trước để xóa file vật lý sau khi xóa bản ghi DB thành công.
        var image = await _mediator.Send(new GetProductImageByIdQuery(imageId), ct);

        var deleted = await _mediator.Send(new DeleteProductImageCommand(imageId), ct);
        if (!deleted)
            return NotFound(ApiResponse.Fail("Không tìm thấy ảnh."));

        if (image is not null)
        {
            var localPath = Path.Combine(_env.ContentRootPath, "wwwroot", image.UrlHinhAnh.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(localPath))
                System.IO.File.Delete(localPath);
        }

        return Ok(ApiResponse.Ok("Đã xóa ảnh."));
    }
}