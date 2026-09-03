using FluentValidation;

namespace CRM.Application.Features.Activities.Commands.DeleteActivity;

public class DeleteActivityCommandValidator : AbstractValidator<DeleteActivityCommand>
{
    public DeleteActivityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
    }
}