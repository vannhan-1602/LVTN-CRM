using FluentValidation;

namespace CRM.Application.Features.Activities.Commands.CreateActivity;

public class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityCommandValidator()
    {
        RuleFor(x => x).Must(x => x.KhachHangId.HasValue || x.LeadId.HasValue)
            .WithMessage("Phải gắn hoạt động với Khách hàng hoặc Lead.");

        RuleFor(x => x.LoaiHoatDong)
            .NotEmpty().WithMessage("Loại hoạt động không được để trống.")
            .MaximumLength(20);

        RuleFor(x => x.NoiDung).MaximumLength(255);
    }
}