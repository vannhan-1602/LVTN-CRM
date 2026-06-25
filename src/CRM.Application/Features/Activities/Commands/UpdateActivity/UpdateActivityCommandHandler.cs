using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Activities.DTOs;
using CRM.Application.Interfaces.Activities;
using MediatR;

namespace CRM.Application.Features.Activities.Commands.UpdateActivity;

public class UpdateActivityCommandHandler : IRequestHandler<UpdateActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _activityRepository;

    public UpdateActivityCommandHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<ActivityDto> Handle(UpdateActivityCommand request, CancellationToken cancellationToken)
    {
        _ = await _activityRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("HoatDong", request.Id);

        return await _activityRepository.UpdateAsync(
            request.Id, request.LoaiHoatDong, request.NoiDung?.Trim(), request.ThoiGianThucHien, cancellationToken);
    }
}