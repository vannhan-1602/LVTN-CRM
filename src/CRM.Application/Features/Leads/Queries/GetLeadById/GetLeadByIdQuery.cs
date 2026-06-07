using CRM.Application.Features.Leads.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Queries.GetLeadById
{
    public record GetLeadByIdQuery(ulong Id) : IRequest<LeadDto>;
}
