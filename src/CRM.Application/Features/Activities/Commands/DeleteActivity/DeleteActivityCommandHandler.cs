using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Activities;
using MediatR;

namespace CRM.Application.Features.Activities.Commands.DeleteActivity;

public class DeleteActivityCommandHandler : IRequestHandler<DeleteActivityCommand, bool>
{
    private readonly IActivityRepository _activityRepository;

    public DeleteActivityCommandHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<bool> Handle(DeleteActivityCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _activityRepository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted) throw new NotFoundException("HoatDong", request.Id);
        return true;
    }
}