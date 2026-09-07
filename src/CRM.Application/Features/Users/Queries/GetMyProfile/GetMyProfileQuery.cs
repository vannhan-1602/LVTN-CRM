using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Users.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Users;
using MediatR;

namespace CRM.Application.Features.Users.Queries.GetMyProfile;

public record GetMyProfileQuery : IRequest<UserDto>;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserDto>
{
    private readonly IUserManagementRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetMyProfileQueryHandler(IUserManagementRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<UserDto> Handle(GetMyProfileQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Không xác định được người dùng.");
        return await _repository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);
    }
}