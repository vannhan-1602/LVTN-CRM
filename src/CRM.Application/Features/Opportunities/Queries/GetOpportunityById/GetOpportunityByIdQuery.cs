using CRM.Application.Features.Opportunities.DTOs;
using MediatR;

namespace CRM.Application.Features.Opportunities.Queries.GetOpportunityById;

public record GetOpportunityByIdQuery(ulong Id) : IRequest<OpportunityDto>;