using FluentValidation;

namespace CRM.Application.Features.Invoices.Queries.GetAllInvoices;

public class GetAllInvoicesQueryValidator : AbstractValidator<GetAllInvoicesQuery>
{
    public GetAllInvoicesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}