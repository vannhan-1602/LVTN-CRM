using FluentValidation;

namespace CRM.Application.Features.Addresses.Commands.CreateAddress;

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(x => x.KhachHangId)
            .GreaterThan(0u).WithMessage("Khách hàng không hợp lệ.");

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
    }
}