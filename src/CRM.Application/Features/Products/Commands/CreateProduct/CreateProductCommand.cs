using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Products.DTOs;
using CRM.Application.Features.Products.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Products;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    uint? LoaiSanPhamId,
    string MaSP,
    string TenSP,
    string? DonVi,
    decimal GiaBan,
    int SoLuongTonBanDau
) : IRequest<ProductDto>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.MaSP)
            .NotEmpty().WithMessage("Mã sản phẩm không được để trống.")
            .MaximumLength(50);

        RuleFor(x => x.TenSP)
            .NotEmpty().WithMessage("Tên sản phẩm không được để trống.")
            .MaximumLength(255);

        RuleFor(x => x.GiaBan)
            .GreaterThanOrEqualTo(0).WithMessage("Giá bán không được âm.");

        RuleFor(x => x.SoLuongTonBanDau)
            .GreaterThanOrEqualTo(0).WithMessage("Số lượng tồn ban đầu không được âm.");

        RuleFor(x => x.DonVi).MaximumLength(50);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private const string AuditTable = "BH_SanPham";
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ICurrentUserService currentUser,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        if (await _productRepository.ExistsMaSPAsync(request.MaSP, null, ct))
            throw new BusinessRuleException($"Mã sản phẩm '{request.MaSP}' đã tồn tại.");

        if (request.LoaiSanPhamId.HasValue &&
            !await _productRepository.LoaiSanPhamExistsAsync(request.LoaiSanPhamId.Value, ct))
            throw new BusinessRuleException("Loại sản phẩm không hợp lệ.");

        var product = new SanPham
        {
            LoaiSanPhamId = request.LoaiSanPhamId,
            MaSP = request.MaSP.Trim(),
            TenSP = request.TenSP.Trim(),
            DonVi = request.DonVi?.Trim(),
            GiaBan = request.GiaBan,
            SoLuongTon = 0, // tồn kho luôn xuất phát từ 0, cộng dồn qua giao dịch kho
            DangKinhDoanh = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _productRepository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Nếu có tồn ban đầu -> tự sinh phiếu nhập kho NhapMua để có lịch sử kho ngay từ đầu
        if (request.SoLuongTonBanDau > 0)
        {
            await _productRepository.AdjustStockAsync(
                created.Id, StockTransactionType.NhapMua, request.SoLuongTonBanDau,
                maChungTu: null, ghiChu: "Tồn kho ban đầu khi tạo sản phẩm",
                nguoiThucHienId: _currentUser.UserId, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var dto = await _productRepository.GetByIdEnrichedAsync(created.Id, ct)
            ?? ProductMapper.ToDto(created);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                oldData: null, newData: dto, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for product {Id}", created.Id); }

        return dto;
    }
}
