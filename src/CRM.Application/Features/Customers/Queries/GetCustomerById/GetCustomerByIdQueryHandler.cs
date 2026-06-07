using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Customers.Mappings;
using CRM.Application.Interfaces.Customers;
using CRM.Domain.Entities.Customers;
using MediatR;

namespace CRM.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(KhachHang), request.Id);

        return CustomerMapper.ToDto(customer);
    }
}
