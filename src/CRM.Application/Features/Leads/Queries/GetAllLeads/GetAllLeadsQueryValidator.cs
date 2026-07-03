using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Queries.GetAllLeads
{
    public class GetAllLeadsQueryValidator : AbstractValidator<GetAllLeadsQuery>
    {
        public GetAllLeadsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        }
    }
}