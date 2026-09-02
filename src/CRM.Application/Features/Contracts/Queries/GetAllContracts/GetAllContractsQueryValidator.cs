using FluentValidation;

namespace CRM.Application.Features.Contracts.Queries.GetAllContracts;

public class GetAllContractsQueryValidator : AbstractValidator<GetAllContractsQuery>
{
    public GetAllContractsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}