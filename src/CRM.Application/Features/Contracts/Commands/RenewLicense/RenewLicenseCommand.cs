using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using CRM.Domain.Entities.Sales;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Contracts.Commands.RenewLicense;

// Gia hạn 1 License đã cấp cho hợp đồng gốc, dựa theo thời hạn của 1 hợp đồng LoaiHopDong=GiaHan
// đã tạo từ hợp đồng gốc đó. KHÔNG tạo License mới, KHÔNG đổi MaLicenseKey — chỉ nối dài
// NgayHetHan (khách vẫn dùng license cũ, đổi key sẽ bắt khách kích hoạt lại phần mềm).
public record RenewLicenseCommand(ulong LicenseId, ulong HopDongGiaHanId) : IRequest<LicenseDto>;

public class RenewLicenseCommandValidator : AbstractValidator<RenewLicenseCommand>
{
    public RenewLicenseCommandValidator()
    {
        RuleFor(x => x.LicenseId).GreaterThan(0UL);
        RuleFor(x => x.HopDongGiaHanId).GreaterThan(0UL);
    }
}

public class RenewLicenseCommandHandler : IRequestHandler<RenewLicenseCommand, LicenseDto>
{
    private readonly ILicenseRepository _licenseRepository;
    private readonly IContractRepository _contractRepository;

    public RenewLicenseCommandHandler(ILicenseRepository licenseRepository, IContractRepository contractRepository)
    {
        _licenseRepository = licenseRepository;
        _contractRepository = contractRepository;
    }

    public async Task<LicenseDto> Handle(RenewLicenseCommand request, CancellationToken ct)
    {
        var license = await _licenseRepository.GetByIdAsync(request.LicenseId, ct)
            ?? throw new NotFoundException("HD_License", request.LicenseId);

        var hopDongGiaHan = await _contractRepository.GetByIdAsync(request.HopDongGiaHanId, ct)
            ?? throw new NotFoundException(nameof(HopDong), request.HopDongGiaHanId);

        if (hopDongGiaHan.LoaiHopDong != "GiaHan")
            throw new BusinessRuleException(
                $"Hợp đồng {hopDongGiaHan.MaHopDong} không phải hợp đồng Gia hạn — không thể dùng để gia hạn License.");

        if (hopDongGiaHan.TrangThai != "DangThucHien")
            throw new BusinessRuleException(
                $"Hợp đồng gia hạn {hopDongGiaHan.MaHopDong} đang ở trạng thái '{hopDongGiaHan.TrangThai}', không thể dùng để gia hạn License.");

        if (hopDongGiaHan.HopDongGocId != license.HopDongId)
            throw new BusinessRuleException(
                "License này không thuộc hợp đồng gốc của hợp đồng gia hạn đã chọn.");

        if (!hopDongGiaHan.NgayKy.HasValue || !hopDongGiaHan.ThoiHan.HasValue)
            throw new BusinessRuleException(
                $"Hợp đồng gia hạn {hopDongGiaHan.MaHopDong} chưa có Ngày ký hoặc Thời hạn, không thể tính ngày hết hạn License mới.");

        var ngayHetHanMoi = hopDongGiaHan.NgayKy.Value.AddMonths(hopDongGiaHan.ThoiHan.Value);

        return await _licenseRepository.RenewAsync(request.LicenseId, ngayHetHanMoi, ct)
            ?? throw new NotFoundException("HD_License", request.LicenseId);
    }
}
