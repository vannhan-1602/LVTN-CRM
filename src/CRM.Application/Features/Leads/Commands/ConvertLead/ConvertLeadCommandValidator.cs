using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.ConvertLead
{
    public class ConvertLeadCommandValidator : AbstractValidator<ConvertLeadCommand>
    {
        public ConvertLeadCommandValidator()
            => RuleFor(x => x.LeadId).GreaterThan(0UL).WithMessage("LeadId không hợp lệ.");
    }
}
