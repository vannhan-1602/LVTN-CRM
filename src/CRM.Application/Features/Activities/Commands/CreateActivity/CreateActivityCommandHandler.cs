using CRM.Application.Features.Activities.DTOs;
using CRM.Application.Interfaces.Activities;
using CRM.Application.Interfaces.Common;
using MediatR;

namespace CRM.Application.Features.Activities.Commands.CreateActivity;

public class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _activityRepository;
    private readonly ICurrentUserService _currentUser;

    public CreateActivityCommandHandler(IActivityRepository activityRepository, ICurrentUserService currentUser)
    {
        _activityRepository = activityRepository;
        _currentUser = currentUser;
    }

    public Task<ActivityDto> Handle(CreateActivityCommand request, CancellationToken cancellationToken) =>
        _activityRepository.AddAsync(
            request.KhachHangId,
            request.LeadId,
            request.LoaiHoatDong,
            request.NoiDung?.Trim(),
            request.ThoiGianThucHien,
            _currentUser.UserId,
            cancellationToken);
}