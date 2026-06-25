using MediatR;

namespace CRM.Application.Features.Activities.Commands.DeleteActivity;

public record DeleteActivityCommand(ulong Id) : IRequest<bool>;