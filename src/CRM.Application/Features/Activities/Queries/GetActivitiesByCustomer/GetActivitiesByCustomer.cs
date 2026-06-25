using CRM.Application.Features.Activities.DTOs;
using MediatR;

namespace CRM.Application.Features.Activities.Queries.GetActivitiesByCustomer;

public record GetActivitiesByCustomerQuery(ulong KhachHangId) : IRequest<List<ActivityDto>>;