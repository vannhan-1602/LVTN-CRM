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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Contracts.Commands.CreateRenewalContract;

// Tạo hợp đồng gia hạn (LoaiHopDong=GiaHan) từ 1 hợp đồng đã có — copy điều khoản
// (KhachHangId, ThoiHan, HinhThucThanhToan), liên kết HopDongGocId về hợp đồng ChinhThuc GỐC
// THẬT (nếu hopDongCu đã là GiaHan thì kế thừa HopDongGocId của nó, không dùng Id của chính
// nó — tránh đứt chuỗi khi gia hạn nhiều cấp liên tiếp, xem chi tiết trong Handle bên dưới),
// và chuyển hợp đồng cũ sang ThanhLy.
//
// LichThanhToans: BẮT BUỘC nếu hợp đồng cũ là TraGop — hợp đồng mới không tự kế thừa lịch
// trả góp của hợp đồng cũ (kỳ hạn mới, có thể đổi số đợt/số tiền), nên phải nhập lại. Nếu
// hợp đồng cũ là ThanhToanMotLan thì bỏ trống — CreateInvoiceCommandHandler sẽ tự phát sinh
// hạn thanh toán khi kế toán xuất hóa đơn đầu tiên cho hợp đồng mới.
public record CreateRenewalContractCommand(
    ulong HopDongCuId, DateOnly? NgayKy, List<LichThanhToanInputDto>? LichThanhToans) : IRequest<ContractDto>;

public class CreateRenewalContractCommandValidator : AbstractValidator<CreateRenewalContractCommand>
{
    public CreateRenewalContractCommandValidator()
    {
        RuleFor(x => x.HopDongCuId).GreaterThan(0UL).WithMessage("Hợp đồng không hợp lệ.");

        RuleForEach(x => x.LichThanhToans)
            .ChildRules(item =>
            {
                item.RuleFor(l => l.SoTien)
                    .GreaterThan(0).WithMessage("Số tiền mỗi đợt phải lớn hơn 0.");
                item.RuleFor(l => l.HanThanhToan)
                    .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                    .WithMessage("Hạn thanh toán không được ở quá khứ.");
            })
            .When(x => x.LichThanhToans is { Count: > 0 });
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

        // Hợp đồng cũ TraGop bắt buộc phải nhập lịch trả góp mới cho kỳ gia hạn — nếu không,
        // hợp đồng mới sẽ có 0 đợt và kế toán sẽ không thể xuất hóa đơn cho nó (dropdown chọn
        // đợt ở FE luôn rỗng).
        var lichThanhToanMoi = request.LichThanhToans ?? new List<LichThanhToanInputDto>();
        if (hopDongCu.HinhThucThanhToan == "TraGop" && lichThanhToanMoi.Count == 0)
            throw new BusinessRuleException(
                "Hợp đồng cũ là trả góp — vui lòng nhập lịch trả góp mới cho kỳ gia hạn.");

        var maHopDong = await _contractRepository.GenerateMaHopDongAsync(ct);

        // HopDongGocId PHẢI luôn trỏ về hợp đồng ChinhThuc gốc thật, KHÔNG phải hợp đồng vừa
        // gia hạn ngay trước đó — vì License luôn được cấp gắn với HopDongId của hợp đồng
        // ChinhThuc gốc (xem CreateLicenseCommand). Nếu hopDongCu bản thân nó đã là GiaHan
        // (gia hạn của 1 gia hạn, VD năm 2 -> năm 3), phải kế thừa HopDongGocId của nó thay vì
        // dùng Id của chính nó — nếu không, từ cấp gia hạn thứ 2 trở đi chuỗi HopDongGocId sẽ bị
        // đứt khỏi hợp đồng gốc thật, khiến RenewLicenseCommand/LicenseSection không tìm thấy
        // (hoặc từ chối) License vốn vẫn thuộc đúng khách hàng/hợp đồng gốc đó.
        var hopDongGocIdThat = hopDongCu.LoaiHopDong == "GiaHan" && hopDongCu.HopDongGocId.HasValue
            ? hopDongCu.HopDongGocId.Value
            : hopDongCu.Id;

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
            HopDongGocId = hopDongGocIdThat,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _contractRepository.AddAsync(hopDongMoi, ct);

        if (hopDongCu.HinhThucThanhToan == "TraGop" && lichThanhToanMoi.Count > 0)
        {
            await _contractRepository.AddLichThanhToanRangeAsync(
                created.Id,
                lichThanhToanMoi.Select(l => (l.SoDot, l.SoTien, l.HanThanhToan)),
                ct);
        }

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