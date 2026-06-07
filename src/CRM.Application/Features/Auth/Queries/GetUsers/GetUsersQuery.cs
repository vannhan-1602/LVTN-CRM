using CRM.Application.Features.Auth.DTOs;
using MediatR;

namespace CRM.Application.Features.Auth.Queries.GetUsers;

public record GetUsersQuery : IRequest<IReadOnlyList<UserSummaryDto>>;
