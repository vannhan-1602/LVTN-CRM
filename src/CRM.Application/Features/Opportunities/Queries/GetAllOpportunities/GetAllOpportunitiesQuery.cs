using CRM.Application.Common.Models;
using CRM.Application.Features.Opportunities.DTOs;
using MediatR;

namespace CRM.Application.Features.Opportunities.Queries.GetAllOpportunities;

public record GetAllOpportunitiesQuery(
    int PageNumber, int PageSize,
    string? Search, string? GiaiDoan,
    ulong? KhachHangId, ulong? LeadId
) : IRequest<PagedResult<OpportunityDto>>;