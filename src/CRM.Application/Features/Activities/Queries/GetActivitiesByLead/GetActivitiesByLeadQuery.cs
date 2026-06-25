using CRM.Application.Features.Activities.DTOs;
using MediatR;

namespace CRM.Application.Features.Activities.Queries.GetActivitiesByLead;

public record GetActivitiesByLeadQuery(ulong LeadId) : IRequest<List<ActivityDto>>;