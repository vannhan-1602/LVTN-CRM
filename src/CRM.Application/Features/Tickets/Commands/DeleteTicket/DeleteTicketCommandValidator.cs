using FluentValidation;

namespace CRM.Application.Features.Tickets.Commands.DeleteTicket
{
    public class DeleteTicketCommandValidator : AbstractValidator<DeleteTicketCommand>
    {
        public DeleteTicketCommandValidator()
            => RuleFor(x => x.Id).GreaterThan(0UL).WithMessage("Id không hợp lệ.");
    }
}