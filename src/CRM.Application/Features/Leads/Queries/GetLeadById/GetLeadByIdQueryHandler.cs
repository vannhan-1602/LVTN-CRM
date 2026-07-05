using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Leads.DTOs;
using CRM.Application.Features.Leads.Mappings;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using MediatR;

namespace CRM.Application.Features.Leads.Queries.GetLeadById;

public class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ICurrentUserService _currentUser;

    public GetLeadByIdQueryHandler(ILeadRepository leadRepository, ICurrentUserService currentUser)
    {
        _leadRepository = leadRepository;
        _currentUser = currentUser;
    }

    public async Task<LeadDto> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.Id, includeDeleted: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Lead), request.Id);

        // Chặn Sale xem Lead không phải của mình (kể cả khi biết Id qua URL trực tiếp)
        EnsureOwnership(lead);

        return LeadMapper.ToDto(lead);
    }

    private void EnsureOwnership(Lead lead)
    {
        if (_currentUser.Role != Roles.Sale) return; // Manager xem được tất cả

        // NhanVienPhuTrachId tham chiếu HT_User.Id 
        if (lead.NhanVienPhuTrachId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền xem dữ liệu của nhân viên khác.");
    }
}