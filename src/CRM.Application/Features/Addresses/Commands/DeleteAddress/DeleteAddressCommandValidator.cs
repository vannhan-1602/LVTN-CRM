using FluentValidation;

namespace CRM.Application.Features.Addresses.Commands.DeleteAddress;

public class DeleteAddressCommandValidator : AbstractValidator<DeleteAddressCommand>
{
    public DeleteAddressCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
    }
}