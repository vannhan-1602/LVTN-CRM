// GetActivitiesByLeadQueryHandler.cs
using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Activities.DTOs;
using CRM.Application.Interfaces.Activities;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Leads;
using MediatR;

namespace CRM.Application.Features.Activities.Queries.GetActivitiesByLead;

public class GetActivitiesByLeadQueryHandler : IRequestHandler<GetActivitiesByLeadQuery, List<ActivityDto>>
{
    private readonly IActivityRepository _activityRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly ICurrentUserService _currentUser;

    public GetActivitiesByLeadQueryHandler(
        IActivityRepository activityRepository,
        ILeadRepository leadRepository,
        ICurrentUserService currentUser)
    {
        _activityRepository = activityRepository;
        _leadRepository = leadRepository;
        _currentUser = currentUser;
    }

    public async Task<List<ActivityDto>> Handle(GetActivitiesByLeadQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == Roles.Sale)
        {
            var lead = await _leadRepository.GetByIdAsync(request.LeadId, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Lead", request.LeadId);
            if (lead.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn không có quyền xem hoạt động của lead này.");
        }

        return await _activityRepository.GetByLeadAsync(request.LeadId, cancellationToken);
    }
}