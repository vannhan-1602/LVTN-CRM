using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Customers.Mappings;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Domain.Entities.Customers;
using MediatR;

namespace CRM.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserService _currentUser;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository, ICurrentUserService currentUser)
    {
        _customerRepository = customerRepository;
        _currentUser = currentUser;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(KhachHang), request.Id);

        //  Chặn Sale xem Customer không phải của mình
        if (_currentUser.Role == Roles.Sale && customer.NhanVienPhuTrachId != _currentUser.NhanSuId)
            throw new ForbiddenException("Bạn không có quyền xem dữ liệu của nhân viên khác.");

        return CustomerMapper.ToDto(customer);
    }
}
