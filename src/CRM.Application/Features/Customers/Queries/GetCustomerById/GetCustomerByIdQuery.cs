using CRM.Application.Features.Customers.DTOs;
using MediatR;

namespace CRM.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(ulong Id) : IRequest<CustomerDto>;
