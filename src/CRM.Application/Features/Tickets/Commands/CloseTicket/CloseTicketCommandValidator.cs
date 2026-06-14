using FluentValidation;

namespace CRM.Application.Features.Tickets.Commands.CloseTicket
{
    public class CloseTicketCommandValidator : AbstractValidator<CloseTicketCommand>
    {
        public CloseTicketCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0UL).WithMessage("Id không hợp lệ.");
            RuleFor(x => x.LyDoDong).MaximumLength(500);
        }
    }
}
