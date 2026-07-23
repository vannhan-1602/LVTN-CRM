// UpdateActivityCommandHandler.cs
using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Activities.DTOs;
using CRM.Application.Interfaces.Activities;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Leads;
using MediatR;

namespace CRM.Application.Features.Activities.Commands.UpdateActivity;

public class UpdateActivityCommandHandler : IRequestHandler<UpdateActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _activityRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly ICurrentUserService _currentUser;

    public UpdateActivityCommandHandler(
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

    public async Task<ActivityDto> Handle(UpdateActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("HoatDong", request.Id);

        await EnsureOwnershipAsync(activity, cancellationToken);

        return await _activityRepository.UpdateAsync(
            request.Id, request.LoaiHoatDong, request.NoiDung?.Trim(), request.ThoiGianThucHien, cancellationToken);
    }

    private async Task EnsureOwnershipAsync(ActivityDto activity, CancellationToken ct)
    {
        if (_currentUser.Role != Roles.Sale) return;

        if (activity.KhachHangId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(activity.KhachHangId.Value, ct);
            if (customer != null && customer.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn không có quyền thao tác trên hoạt động của nhân viên khác.");
        }
        if (activity.LeadId.HasValue)
        {
            var lead = await _leadRepository.GetByIdAsync(activity.LeadId.Value, cancellationToken: ct);
            if (lead != null && lead.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn không có quyền thao tác trên hoạt động của nhân viên khác.");
        }
    }
}