using CRM.Application.Features.Activities.DTOs;
using CRM.Application.Interfaces.Activities;
using MediatR;

namespace CRM.Application.Features.Activities.Queries.GetActivitiesByCustomer;

public class GetActivitiesByCustomerQueryHandler : IRequestHandler<GetActivitiesByCustomerQuery, List<ActivityDto>>
{
    private readonly IActivityRepository _activityRepository;

    public GetActivitiesByCustomerQueryHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public Task<List<ActivityDto>> Handle(GetActivitiesByCustomerQuery request, CancellationToken cancellationToken) =>
        _activityRepository.GetByKhachHangAsync(request.KhachHangId, cancellationToken);
}