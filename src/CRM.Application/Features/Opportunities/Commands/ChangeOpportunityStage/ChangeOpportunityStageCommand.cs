using CRM.Application.Features.Opportunities.DTOs;
using MediatR;

namespace CRM.Application.Features.Opportunities.Commands.ChangeOpportunityStage;

public record ChangeOpportunityStageCommand(ulong Id, string GiaiDoan, string? GhiChu)
    : IRequest<OpportunityDto>;