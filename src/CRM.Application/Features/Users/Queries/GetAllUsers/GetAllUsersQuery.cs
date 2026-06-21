using CRM.Application.Features.Users.DTOs;
using CRM.Application.Interfaces.Users;
using MediatR;

namespace CRM.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<List<UserDto>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IUserManagementRepository _repository;
    public GetAllUsersQueryHandler(IUserManagementRepository repository) => _repository = repository;

    public Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken ct) =>
        _repository.GetAllAsync(ct);
}
