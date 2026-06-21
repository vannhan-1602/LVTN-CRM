using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Quotes;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Contracts.Commands.CreateContractFromQuote;


// Tạo Hợp đồng từ một Báo giá đã ở trạng thái "ChapNhan". Đây là điểm nối chính
//của chuỗi nghiệp vụ Sản phẩm → Báo giá → Hợp đồng 

public record CreateContractFromQuoteCommand(
    ulong BaoGiaId, DateOnly? NgayKy, int? ThoiHan
) : IRequest<ContractDto>;

public class CreateContractFromQuoteCommandValidator : AbstractValidator<CreateContractFromQuoteCommand>
{
    public CreateContractFromQuoteCommandValidator()
    {
        RuleFor(x => x.BaoGiaId).GreaterThan(0UL).WithMessage("Báo giá không hợp lệ.");
        RuleFor(x => x.ThoiHan).GreaterThan(0).When(x => x.ThoiHan.HasValue)
            .WithMessage("Thời hạn hợp đồng phải lớn hơn 0 tháng.");
    }
}

public class CreateContractFromQuoteCommandHandler
    : IRequestHandler<CreateContractFromQuoteCommand, ContractDto>
{
    private const string AuditTable = "HD_HopDong";
    private readonly IContractRepository _contractRepository;
    private readonly IQuoteRepository _quoteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateContractFromQuoteCommandHandler> _logger;

    public CreateContractFromQuoteCommandHandler(
        IContractRepository contractRepository, IQuoteRepository quoteRepository,
        IUnitOfWork unitOfWork, IAuditLogPublisher auditLogPublisher,
        ICurrentUserService currentUser, ILogger<CreateContractFromQuoteCommandHandler> logger)
    {
        _contractRepository = contractRepository;
        _quoteRepository = quoteRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ContractDto> Handle(CreateContractFromQuoteCommand request, CancellationToken ct)
    {
        var quote = await _quoteRepository.GetByIdAsync(request.BaoGiaId, ct)
            ?? throw new NotFoundException(nameof(BaoGia), request.BaoGiaId);

        if (_currentUser.Role == Roles.Sale && quote.NhanVienId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền tạo hợp đồng từ báo giá của nhân viên khác.");

        if (quote.TrangThai != QuoteStatus.ChapNhan)
            throw new BusinessRuleException("Chỉ có thể tạo hợp đồng từ báo giá đã được chấp nhận.");

        if (await _contractRepository.ExistsForBaoGiaAsync(request.BaoGiaId, ct))
            throw new BusinessRuleException("Báo giá này đã được dùng để tạo hợp đồng khác.");

        var maHopDong = await _contractRepository.GenerateMaHopDongAsync(ct);

        var contract = new HopDong
        {
            MaHopDong = maHopDong,
            KhachHangId = quote.KhachHangId,
            BaoGiaGocId = quote.Id,
            NgayKy = request.NgayKy ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ThoiHan = request.ThoiHan,
            TrangThai = ContractStatus.DangThucHien,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _contractRepository.AddAsync(contract, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = await _contractRepository.GetByIdEnrichedAsync(created.Id, ct)
            ?? throw new BusinessRuleException("Tạo hợp đồng thất bại.");

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                oldData: null, newData: dto, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for contract {Id}", created.Id); }

        return dto;
    }
}
