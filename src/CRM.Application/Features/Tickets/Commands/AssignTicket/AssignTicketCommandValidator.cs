using FluentValidation;

namespace CRM.Application.Features.Tickets.Commands.AssignTicket
{
    public class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
    {
        public AssignTicketCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0UL).WithMessage("Id không hợp lệ.");
            RuleFor(x => x.NhanVienXuLyId).GreaterThan(0U).WithMessage("Nhân viên xử lý không hợp lệ.");
        }
    }
}