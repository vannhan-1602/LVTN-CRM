// DeleteActivityCommandHandler.cs
using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Activities;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Leads;
using MediatR;

namespace CRM.Application.Features.Activities.Commands.DeleteActivity;

public class DeleteActivityCommandHandler : IRequestHandler<DeleteActivityCommand, bool>
{
    private readonly IActivityRepository _activityRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly ICurrentUserService _currentUser;

    public DeleteActivityCommandHandler(
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

    public async Task<bool> Handle(DeleteActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activityRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("HoatDong", request.Id);

        if (_currentUser.Role == Roles.Sale)
        {
            if (activity.KhachHangId.HasValue)
            {
                var customer = await _customerRepository.GetByIdAsync(activity.KhachHangId.Value, cancellationToken);
                if (customer != null && customer.NhanVienPhuTrachId != _currentUser.UserId)
                    throw new ForbiddenException("Bạn không có quyền xóa hoạt động của nhân viên khác.");
            }
            if (activity.LeadId.HasValue)
            {
                var lead = await _leadRepository.GetByIdAsync(activity.LeadId.Value, cancellationToken: cancellationToken);
                if (lead != null && lead.NhanVienPhuTrachId != _currentUser.UserId)
                    throw new ForbiddenException("Bạn không có quyền xóa hoạt động của nhân viên khác.");
            }
        }

        var deleted = await _activityRepository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted) throw new NotFoundException("HoatDong", request.Id);
        return true;
    }
}