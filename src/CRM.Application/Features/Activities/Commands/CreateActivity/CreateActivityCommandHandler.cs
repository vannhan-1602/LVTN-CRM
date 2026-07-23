// CreateActivityCommandHandler.cs
using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Activities.DTOs;
using CRM.Application.Interfaces.Activities;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Leads;
using MediatR;

namespace CRM.Application.Features.Activities.Commands.CreateActivity;

public class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _activityRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly ICurrentUserService _currentUser;

    public CreateActivityCommandHandler(
        IActivityRepository activityRepository,
        ICustomerRepository customerRepository,
        ILeadRepository leadRepository,
        ICurrentUserService currentUser)
    {
        _activityRepository = activityRepository;
        _customerRepository = customerRepository;
        _leadRepository = leadRepository;
        _currentUser = currentUser;
    }

    public async Task<ActivityDto> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == Roles.Sale)
        {
            if (request.KhachHangId.HasValue)
            {
                var customer = await _customerRepository.GetByIdAsync(request.KhachHangId.Value, cancellationToken)
                    ?? throw new NotFoundException("Khách hàng", request.KhachHangId.Value);
                if (customer.NhanVienPhuTrachId != _currentUser.UserId)
                    throw new ForbiddenException("Bạn không có quyền ghi nhận hoạt động cho khách hàng của nhân viên khác.");
            }
            if (request.LeadId.HasValue)
            {
                var lead = await _leadRepository.GetByIdAsync(request.LeadId.Value, cancellationToken: cancellationToken)
                    ?? throw new NotFoundException("Lead", request.LeadId.Value);
                if (lead.NhanVienPhuTrachId != _currentUser.UserId)
                    throw new ForbiddenException("Bạn không có quyền ghi nhận hoạt động cho lead của nhân viên khác.");
            }
        }

        return await _activityRepository.AddAsync(
            request.KhachHangId, request.LeadId, request.LoaiHoatDong,
            request.NoiDung?.Trim(), request.ThoiGianThucHien, _currentUser.UserId, cancellationToken);
    }
}