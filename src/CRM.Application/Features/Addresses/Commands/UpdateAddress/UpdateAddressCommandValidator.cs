using CRM.Application.Interfaces.Addresses;
using FluentValidation;

namespace CRM.Application.Features.Addresses.Commands.UpdateAddress;

public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator(IAddressRepository addressRepository)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0u).WithMessage("ID địa chỉ không hợp lệ.");

        RuleFor(x => x.LoaiDiaChi)
            .NotEmpty().WithMessage("Loại địa chỉ không được để trống.")
            .Must(x => x == "Office" || x == "Billing" || x == "Shipping")
            .WithMessage("Loại địa chỉ phải là Office, Billing hoặc Shipping.");

        RuleFor(x => x.TinhThanhId)
            .NotNull().WithMessage("Vui lòng chọn Tỉnh/Thành phố.")
            .GreaterThan(0u).WithMessage("Tỉnh/Thành phố không hợp lệ.");

        RuleFor(x => x.PhuongXaId)
            .NotNull().WithMessage("Vui lòng chọn Phường/Xã.")
            .GreaterThan(0u).WithMessage("Phường/Xã không hợp lệ.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                !cmd.TinhThanhId.HasValue || !cmd.PhuongXaId.HasValue ||
                await addressRepository.PhuongXaBelongsToTinhThanhAsync(cmd.PhuongXaId.Value, cmd.TinhThanhId.Value, ct))
            .WithMessage("Phường/Xã đã chọn không thuộc Tỉnh/Thành phố đã chọn.")
            .When(x => x.TinhThanhId.HasValue && x.PhuongXaId.HasValue);
    }
}