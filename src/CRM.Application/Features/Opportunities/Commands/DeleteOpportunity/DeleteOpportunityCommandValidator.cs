using FluentValidation;

namespace CRM.Application.Features.Opportunities.Commands.DeleteOpportunity;

public class DeleteOpportunityCommandValidator : AbstractValidator<DeleteOpportunityCommand>
{
    public DeleteOpportunityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
    }
}