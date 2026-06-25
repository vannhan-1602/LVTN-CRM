using CRM.Application.Features.Activities.DTOs;
using CRM.Application.Interfaces.Activities;
using MediatR;

namespace CRM.Application.Features.Activities.Queries.GetActivitiesByLead;

public class GetActivitiesByLeadQueryHandler : IRequestHandler<GetActivitiesByLeadQuery, List<ActivityDto>>
{
    private readonly IActivityRepository _activityRepository;

    public GetActivitiesByLeadQueryHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public Task<List<ActivityDto>> Handle(GetActivitiesByLeadQuery request, CancellationToken cancellationToken) =>
        _activityRepository.GetByLeadAsync(request.LeadId, cancellationToken);
}