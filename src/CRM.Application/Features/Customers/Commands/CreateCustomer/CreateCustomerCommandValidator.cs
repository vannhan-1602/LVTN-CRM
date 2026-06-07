using CRM.Application.Interfaces.Customers;
using FluentValidation;

namespace CRM.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator(ICustomerRepository customerRepository)
    {
        RuleFor(x => x.TenKhachHang)
            .NotEmpty().WithMessage("Tên khách hàng không được để trống.")
            .MaximumLength(100).WithMessage("Tên khách hàng không được vượt quá 100 ký tự.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.SoDienThoai)
            .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.SoDienThoai));

        RuleFor(x => x.MaSoThue)
            .MaximumLength(50).WithMessage("Mã số thuế không được vượt quá 50 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.MaSoThue));

        RuleFor(x => x.LoaiKhachHangId)
            .MustAsync(async (id, ct) => !id.HasValue || await customerRepository.LoaiKhachHangExistsAsync(id.Value, ct))
            .WithMessage("Loại khách hàng không tồn tại.")
            .When(x => x.LoaiKhachHangId.HasValue);

        RuleFor(x => x.TinhTrangId)
            .MustAsync(async (id, ct) => !id.HasValue || await customerRepository.TinhTrangKhachHangExistsAsync(id.Value, ct))
            .WithMessage("Tình trạng khách hàng không tồn tại.")
            .When(x => x.TinhTrangId.HasValue);
    }
}
