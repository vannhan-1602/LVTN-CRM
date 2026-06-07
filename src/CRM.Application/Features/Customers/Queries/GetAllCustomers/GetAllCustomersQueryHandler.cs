using CRM.Application.Common.Models;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Customers.Mappings;
using CRM.Application.Interfaces.Customers;
using MediatR;

namespace CRM.Application.Features.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetAllCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedResult<CustomerDto>> Handle(
        GetAllCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _customerRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.Search,
            cancellationToken);

        return new PagedResult<CustomerDto>
        {
            Items = result.Items.Select(CustomerMapper.ToDto).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}
