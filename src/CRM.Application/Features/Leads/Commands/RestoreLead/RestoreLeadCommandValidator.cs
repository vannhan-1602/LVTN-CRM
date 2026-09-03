using FluentValidation;

namespace CRM.Application.Features.Leads.Commands.RestoreLead;

public class RestoreLeadCommandValidator : AbstractValidator<RestoreLeadCommand>
{
    public RestoreLeadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
    }
}