using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.DeleteLead
{
    public class DeleteLeadCommandValidator : AbstractValidator<DeleteLeadCommand>
    {
        public DeleteLeadCommandValidator()
            => RuleFor(x => x.Id).GreaterThan(0UL).WithMessage("Id không hợp lệ.");
    }
}
