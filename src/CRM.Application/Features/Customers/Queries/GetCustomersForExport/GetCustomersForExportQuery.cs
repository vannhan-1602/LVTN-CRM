using CRM.Application.Common.Constants;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using MediatR;

namespace CRM.Application.Features.Customers.Queries.GetCustomersForExport;

public record GetCustomersForExportQuery(
    string? Search, ushort? LoaiKhachHangId, ushort? TinhTrangId
) : IRequest<List<CustomerDto>>;

public class GetCustomersForExportQueryHandler : IRequestHandler<GetCustomersForExportQuery, List<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserService _currentUser;

    public GetCustomersForExportQueryHandler(ICustomerRepository customerRepository, ICurrentUserService currentUser)
    {
        _customerRepository = customerRepository;
        _currentUser = currentUser;
    }

    public Task<List<CustomerDto>> Handle(GetCustomersForExportQuery request, CancellationToken ct)
    {
        uint? ownerUserId = _currentUser.Role == Roles.Sale ? _currentUser.UserId : null;

        return _customerRepository.GetForExportAsync(
            request.Search, request.LoaiKhachHangId, request.TinhTrangId, ownerUserId, ct);
    }
}