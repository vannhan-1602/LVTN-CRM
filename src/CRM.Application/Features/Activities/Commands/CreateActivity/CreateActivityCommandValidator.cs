using FluentValidation;

namespace CRM.Application.Features.Activities.Commands.CreateActivity;

public class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityCommandValidator()
    {
       
        RuleFor(x => x).Must(x => x.KhachHangId.HasValue ^ x.LeadId.HasValue)
            .WithMessage("Hoạt động phải gắn với đúng một Khách hàng hoặc một Lead, không được chọn cả hai hoặc bỏ trống cả hai.");

        RuleFor(x => x.LoaiHoatDong)
            .NotEmpty().WithMessage("Loại hoạt động không được để trống.")
            .MaximumLength(20);

        RuleFor(x => x.NoiDung).MaximumLength(255);
    }
}