using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using System;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Contracts.Commands.CreateRenewalContract;

// Tạo hợp đồng gia hạn (LoaiHopDong=GiaHan) từ 1 hợp đồng đã có — copy điều khoản
// (KhachHangId, ThoiHan, HinhThucThanhToan), liên kết HopDongGocId về hợp đồng cũ,
// và chuyển hợp đồng cũ sang ThanhLy.
public record CreateRenewalContractCommand(ulong HopDongCuId, DateOnly? NgayKy) : IRequest<ContractDto>;

public class CreateRenewalContractCommandValidator : AbstractValidator<CreateRenewalContractCommand>
{
    public CreateRenewalContractCommandValidator()
    {
        RuleFor(x => x.HopDongCuId).GreaterThan(0UL).WithMessage("Hợp đồng không hợp lệ.");
    }
}

public class CreateRenewalContractCommandHandler
    : IRequestHandler<CreateRenewalContractCommand, ContractDto>
{
    private const string AuditTable = "HD_HopDong";
    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateRenewalContractCommandHandler> _logger;

    public CreateRenewalContractCommandHandler(
        IContractRepository contractRepository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ICurrentUserService currentUser,
        ILogger<CreateRenewalContractCommandHandler> logger)
    {
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ContractDto> Handle(CreateRenewalContractCommand request, CancellationToken ct)
    {
        var hopDongCu = await _contractRepository.GetByIdAsync(request.HopDongCuId, ct)
            ?? throw new NotFoundException(nameof(HopDong), request.HopDongCuId);

        if (hopDongCu.TrangThai == ContractStatus.ThanhLy)
            throw new BusinessRuleException("Hợp đồng đã thanh lý, không thể gia hạn.");

        var maHopDong = await _contractRepository.GenerateMaHopDongAsync(ct);

        var hopDongMoi = new HopDong
        {
            MaHopDong = maHopDong,
            KhachHangId = hopDongCu.KhachHangId,
            BaoGiaGocId = hopDongCu.BaoGiaGocId,
            NgayKy = request.NgayKy ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ThoiHan = hopDongCu.ThoiHan,
            HinhThucThanhToan = hopDongCu.HinhThucThanhToan,
            TrangThai = ContractStatus.DangThucHien,
            LoaiHopDong = "GiaHan",
            HopDongGocId = hopDongCu.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _contractRepository.AddAsync(hopDongMoi, ct);

        // Hợp đồng cũ chuyển sang Thanh lý — đã được thay thế bởi hợp đồng gia hạn.
        await _contractRepository.UpdateStatusAsync(hopDongCu.Id, ContractStatus.ThanhLy, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        var dto = await _contractRepository.GetByIdEnrichedAsync(created.Id, ct)
            ?? throw new BusinessRuleException("Tạo hợp đồng gia hạn thất bại.");

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                oldData: null, newData: dto, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for renewal contract {Id}", created.Id); }

        return dto;
    }
}
