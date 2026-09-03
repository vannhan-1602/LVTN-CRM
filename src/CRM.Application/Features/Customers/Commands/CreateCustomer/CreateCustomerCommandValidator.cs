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
            .MustAsync(async (email, ct) =>
            {
                var matches = await customerRepository.FindDuplicatesAsync(email, null, null, null, ct);
                return matches.Count == 0;
            })
            .WithMessage("Email này đã tồn tại ở một khách hàng khác.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.SoDienThoai)
            .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.SoDienThoai));

        RuleFor(x => x.MaSoThue)
            .MaximumLength(50).WithMessage("Mã số thuế không được vượt quá 50 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.MaSoThue));

        RuleFor(x => x.NgaySinh)
            .LessThanOrEqualTo(x => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Ngày sinh không được ở tương lai.")
            .When(x => x.NgaySinh.HasValue);

        RuleFor(x => x.NgayThanhLap)
            .LessThanOrEqualTo(x => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Ngày thành lập không được ở tương lai.")
            .When(x => x.NgayThanhLap.HasValue);
        RuleFor(x => x).CustomAsync(async (command, context, ct) =>
        {
            var (loaiOk, tinhTrangOk, hangOk) = await customerRepository.ValidateLookupIdsAsync(
                command.LoaiKhachHangId, command.TinhTrangId, command.HangKhachHangId, ct);

            if (command.LoaiKhachHangId.HasValue && !loaiOk)
                context.AddFailure(nameof(command.LoaiKhachHangId), "Loại khách hàng không tồn tại.");

            if (command.TinhTrangId.HasValue && !tinhTrangOk)
                context.AddFailure(nameof(command.TinhTrangId), "Tình trạng khách hàng không tồn tại.");

            if (command.HangKhachHangId.HasValue && !hangOk)
                context.AddFailure(nameof(command.HangKhachHangId), "Hạng khách hàng không tồn tại.");
        });
    }
}