using FluentValidation;

namespace CRM.Application.Features.Activities.Commands.UpdateActivity;

public class UpdateActivityCommandValidator : AbstractValidator<UpdateActivityCommand>
{
    public UpdateActivityCommandValidator()
    {
        RuleFor(x => x.LoaiHoatDong).NotEmpty().MaximumLength(20);
        RuleFor(x => x.NoiDung).MaximumLength(255);
    }
}