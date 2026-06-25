using CRM.Application.Features.Opportunities.DTOs;
using MediatR;

namespace CRM.Application.Features.Opportunities.Queries.GetOpportunitySummary;

public record GetOpportunitySummaryQuery : IRequest<OpportunitySummaryDto>;