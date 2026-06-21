using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Users.DTOs;
using CRM.Application.Interfaces.Users;
using MediatR;

namespace CRM.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(uint Id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserManagementRepository _repository;
    public GetUserByIdQueryHandler(IUserManagementRepository repository) => _repository = repository;

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken ct) =>
        await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("User", request.Id);
}
