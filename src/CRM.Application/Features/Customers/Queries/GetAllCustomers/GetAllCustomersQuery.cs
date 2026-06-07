using CRM.Application.Common.Models;
using CRM.Application.Features.Customers.DTOs;
using MediatR;

namespace CRM.Application.Features.Customers.Queries.GetAllCustomers;

public record GetAllCustomersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null) : IRequest<PagedResult<CustomerDto>>;
