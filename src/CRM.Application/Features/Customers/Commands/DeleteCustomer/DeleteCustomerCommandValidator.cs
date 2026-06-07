using FluentValidation;

namespace CRM.Application.Features.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0UL).WithMessage("Id khách hàng không hợp lệ.");
    }
}
