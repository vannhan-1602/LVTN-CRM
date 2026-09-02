using FluentValidation;

namespace CRM.Application.Features.Quotes.Queries.GetAllQuotes;

public class GetAllQuotesQueryValidator : AbstractValidator<GetAllQuotesQuery>
{
    public GetAllQuotesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}