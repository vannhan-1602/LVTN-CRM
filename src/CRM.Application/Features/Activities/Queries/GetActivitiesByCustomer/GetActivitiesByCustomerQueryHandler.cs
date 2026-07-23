// GetActivitiesByCustomerQueryHandler.cs
using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Activities.DTOs;
using CRM.Application.Interfaces.Activities;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using MediatR;

namespace CRM.Application.Features.Activities.Queries.GetActivitiesByCustomer;

public class GetActivitiesByCustomerQueryHandler : IRequestHandler<GetActivitiesByCustomerQuery, List<ActivityDto>>
{
    private readonly IActivityRepository _activityRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserService _currentUser;

    public GetActivitiesByCustomerQueryHandler(
        IActivityRepository activityRepository,
        ICustomerRepository customerRepository,
        ICurrentUserService currentUser)
    {
        _activityRepository = activityRepository;
        _customerRepository = customerRepository;
        _currentUser = currentUser;
    }

    public async Task<List<ActivityDto>> Handle(GetActivitiesByCustomerQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == Roles.Sale)
        {
            var customer = await _customerRepository.GetByIdAsync(request.KhachHangId, cancellationToken)
                ?? throw new NotFoundException("Khách hàng", request.KhachHangId);
            if (customer.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn không có quyền xem hoạt động của khách hàng này.");
        }

        return await _activityRepository.GetByKhachHangAsync(request.KhachHangId, cancellationToken);
    }
}