using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Products.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Products;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Products.Commands.UpdateStock;

public record UpdateStockCommand(
    uint SanPhamId,
    string LoaiGiaoDich,
    int SoLuong,
    string? MaChungTu,
    string? GhiChu
) : IRequest<StockTransactionResultDto>;

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.SanPhamId).GreaterThan(0U);

        RuleFor(x => x.LoaiGiaoDich)
            .NotEmpty().WithMessage("Loại giao dịch không được để trống.")
            .Must(v => StockTransactionType.All.Contains(v))
            .WithMessage("Loại giao dịch không hợp lệ.");

        RuleFor(x => x.SoLuong)
            .GreaterThan(0).WithMessage("Số lượng giao dịch phải lớn hơn 0.");

        RuleFor(x => x.MaChungTu).MaximumLength(50);
        RuleFor(x => x.GhiChu).MaximumLength(255);
    }
}

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, StockTransactionResultDto>
{
    private const string AuditTable = "Kho_TheKho";
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateStockCommandHandler> _logger;

    public UpdateStockCommandHandler(
        IProductRepository productRepository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ICurrentUserService currentUser,
        ILogger<UpdateStockCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<StockTransactionResultDto> Handle(UpdateStockCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.SanPhamId, ct)
            ?? throw new NotFoundException(nameof(SanPham), request.SanPhamId);

        // Số lượng giao dịch luôn nhập dương; dấu +/- do loại giao dịch quyết định
        var soLuongThayDoi = StockTransactionType.DecreaseTypes.Contains(request.LoaiGiaoDich)
            ? -request.SoLuong
            : request.SoLuong;

        var tonHienTai = await _productRepository.GetCurrentStockAsync(request.SanPhamId, ct);
        if (tonHienTai + soLuongThayDoi < 0)
            throw new BusinessRuleException(
                $"Không đủ tồn kho. Tồn hiện tại: {tonHienTai}, yêu cầu xuất: {request.SoLuong}.");

        var result = await _productRepository.AdjustStockAsync(
            request.SanPhamId, request.LoaiGiaoDich, soLuongThayDoi,
            request.MaChungTu, request.GhiChu, _currentUser.UserId, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, result.Transaction.Id, "INSERT",
                oldData: null, newData: result, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for stock transaction {Id}", result.Transaction.Id); }

        return result;
    }
}
