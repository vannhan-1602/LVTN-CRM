using CRM.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.UpdateLead
{
    public class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
    {
        public UpdateLeadCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0UL).WithMessage("Id không hợp lệ.");
            RuleFor(x => x.TenLead)
                .NotEmpty().WithMessage("Tên lead không được để trống.")
                .MaximumLength(150);
            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.TinhTrang)
                .Must(t => LeadTinhTrang.All.Contains(t))
                .WithMessage("Tình trạng không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.TinhTrang));
        }
    }
}
