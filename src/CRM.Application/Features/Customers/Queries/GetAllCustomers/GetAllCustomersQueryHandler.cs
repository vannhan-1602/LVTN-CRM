using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using MediatR;

namespace CRM.Application.Features.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQueryHandler
    : IRequestHandler<GetAllCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserService _currentUser;

    public GetAllCustomersQueryHandler(ICustomerRepository customerRepository, ICurrentUserService currentUser)
    {
        _customerRepository = customerRepository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<CustomerDto>> Handle(
        GetAllCustomersQuery request,
        CancellationToken cancellationToken)
    {
        // Sale chỉ xem Customer mình phụ trách. Manager xem toàn đội.
       
        uint? ownerUserId = _currentUser.Role == Roles.Sale ? _currentUser.UserId : null;

        var result = await _customerRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.LoaiKhachHangId,
            request.TinhTrangId,
            ownerUserId,
            request.IsDeleted,
            cancellationToken);

        return new PagedResult<CustomerDto>
        {
            Items = result.Items,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}