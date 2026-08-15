using FluentValidation;

namespace CRM.Application.Features.Leads.Commands.AssignLead;

public class AssignLeadCommandValidator : AbstractValidator<AssignLeadCommand>
{
    public AssignLeadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL).WithMessage("Id không hợp lệ.");
        RuleFor(x => x.NhanVienPhuTrachId).GreaterThan(0U).WithMessage("Nhân viên phụ trách không hợp lệ.");
    }
}