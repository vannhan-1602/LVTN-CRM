using CRM.Application.Features.Addresses.DTOs;
using MediatR;

namespace CRM.Application.Features.Addresses.Queries.GetAddressesByCustomer;

public record GetAddressesByCustomerQuery(ulong KhachHangId) : IRequest<List<AddressDto>>;