using FluentValidation;

namespace CRM.Application.Features.Leads.Commands.CreatePublicLead;

public class CreatePublicLeadCommandValidator : AbstractValidator<CreatePublicLeadCommand>
{
    public CreatePublicLeadCommandValidator()
    {
        RuleFor(x => x.TenLead)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(150).WithMessage("Họ tên không quá 150 ký tự.");

        RuleFor(x => x.SoDienThoai)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .MaximumLength(20).WithMessage("Số điện thoại không quá 20 ký tự.")
            .Matches(@"^[0-9+\-\s()]+$").WithMessage("Số điện thoại chỉ được chứa số và ký tự +-() .");

        RuleFor(x => x.TenCongTy)
            .MaximumLength(150).WithMessage("Tên công ty không quá 150 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.TenCongTy));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(100).WithMessage("Email không quá 100 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}