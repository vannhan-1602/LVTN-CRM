using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.DeleteLead
{
    public record DeleteLeadCommand(ulong Id) : IRequest<bool>;
}
