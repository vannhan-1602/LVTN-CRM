using FluentValidation;

namespace CRM.Application.Features.Loyalty.Commands.RedeemVoucher;


public class RedeemVoucherCommandValidator : AbstractValidator<RedeemVoucherCommand>
{
    public RedeemVoucherCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token không hợp lệ.")
            .MaximumLength(500).WithMessage("Token không hợp lệ.");
    }
}