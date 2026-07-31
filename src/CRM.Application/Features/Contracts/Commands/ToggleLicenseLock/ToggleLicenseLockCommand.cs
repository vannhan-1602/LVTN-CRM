using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Contracts.Commands.ToggleLicenseLock;

// Khóa (TamKhoa) / mở khóa (DangHoatDong) 1 License. Không áp dụng cho License đã HetHan —
// trường hợp đó phải Gia hạn (RenewLicense) để kích hoạt lại, không phải mở khóa.
public record ToggleLicenseLockCommand(ulong LicenseId, bool Khoa) : IRequest<LicenseDto>;

public class ToggleLicenseLockCommandValidator : AbstractValidator<ToggleLicenseLockCommand>
{
    public ToggleLicenseLockCommandValidator() => RuleFor(x => x.LicenseId).GreaterThan(0UL);
}

public class ToggleLicenseLockCommandHandler : IRequestHandler<ToggleLicenseLockCommand, LicenseDto>
{
    private readonly ILicenseRepository _licenseRepository;
    public ToggleLicenseLockCommandHandler(ILicenseRepository licenseRepository) =>
        _licenseRepository = licenseRepository;

    public async Task<LicenseDto> Handle(ToggleLicenseLockCommand request, CancellationToken ct)
    {
        var license = await _licenseRepository.GetByIdAsync(request.LicenseId, ct)
            ?? throw new NotFoundException("HD_License", request.LicenseId);

        if (license.TrangThai == "HetHan")
            throw new BusinessRuleException(
                "License đã hết hạn — không thể khóa/mở khóa, vui lòng dùng chức năng Gia hạn.");

        return await _licenseRepository.ToggleLockAsync(request.LicenseId, request.Khoa, ct)
            ?? throw new NotFoundException("HD_License", request.LicenseId);
    }
}
