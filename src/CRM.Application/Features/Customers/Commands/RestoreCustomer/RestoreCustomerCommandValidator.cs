using FluentValidation;

namespace CRM.Application.Features.Customers.Commands.RestoreCustomer;

public class RestoreCustomerCommandValidator : AbstractValidator<RestoreCustomerCommand>
{
    public RestoreCustomerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
    }
}