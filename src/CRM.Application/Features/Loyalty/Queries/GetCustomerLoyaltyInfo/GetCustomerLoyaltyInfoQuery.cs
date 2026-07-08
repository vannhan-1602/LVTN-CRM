using CRM.Application.Features.Loyalty.DTOs;
using MediatR;

namespace CRM.Application.Features.Loyalty.Queries.GetCustomerLoyaltyInfo;

public record GetCustomerLoyaltyInfoQuery(ulong KhachHangId) : IRequest<CustomerLoyaltyInfoDto>;
