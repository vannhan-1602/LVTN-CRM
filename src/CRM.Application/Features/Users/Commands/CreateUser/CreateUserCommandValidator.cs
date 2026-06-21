using FluentValidation;

namespace CRM.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
            .MinimumLength(4).WithMessage("Tên đăng nhập tối thiểu 4 ký tự.")
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_.]+$").WithMessage("Tên đăng nhập chỉ gồm chữ, số, dấu chấm, gạch dưới.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu tối thiểu 6 ký tự.");

        RuleFor(x => x.RoleId).GreaterThan(0U).WithMessage("Vui lòng chọn vai trò.");

        RuleFor(x => x.HoTen)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.SoDienThoai)
            .MaximumLength(20);
    }
}
