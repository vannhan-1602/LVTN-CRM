using MediatR;

namespace CRM.Application.Features.Opportunities.Commands.DeleteOpportunity;

public record DeleteOpportunityCommand(ulong Id) : IRequest<bool>;