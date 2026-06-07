using CRM.Application.Common.Models;
using CRM.Application.Features.Leads.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Queries.GetAllLeads
{
    public record GetAllLeadsQuery(int PageNumber, int PageSize, string? Search) : IRequest<PagedResult<LeadDto>>;
}
