using FluentValidation;

namespace CRM.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
            .MaximumLength(50).WithMessage("Tên đăng nhập không được vượt quá 50 ký tự.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MaximumLength(255).WithMessage("Mật khẩu không hợp lệ.");
    }
}
