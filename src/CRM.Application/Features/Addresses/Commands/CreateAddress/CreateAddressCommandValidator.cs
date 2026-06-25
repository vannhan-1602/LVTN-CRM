using FluentValidation;

namespace CRM.Application.Features.Addresses.Commands.CreateAddress;

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(x => x.LoaiDiaChi).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DiaChiChiTiet).MaximumLength(150);
        RuleFor(x => x.TinhThanh).MaximumLength(50);
        RuleFor(x => x.QuanHuyen).MaximumLength(50);
        RuleFor(x => x.PhuongXa).MaximumLength(50);
    }
}