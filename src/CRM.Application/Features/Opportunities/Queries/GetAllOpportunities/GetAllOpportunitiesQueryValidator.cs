using FluentValidation;

namespace CRM.Application.Features.Opportunities.Queries.GetAllOpportunities;

public class GetAllOpportunitiesQueryValidator : AbstractValidator<GetAllOpportunitiesQuery>
{
    public GetAllOpportunitiesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}