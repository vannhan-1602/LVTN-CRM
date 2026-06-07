using CRM.Application.Features.Auth.DTOs;
using CRM.Application.Interfaces.Auth;
using MediatR;

namespace CRM.Application.Features.Auth.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserSummaryDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllWithRolesAsync(cancellationToken);

        return users
            .Select(u => new UserSummaryDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.RoleName,
                TrangThai = u.TrangThai,
                HoTen = u.HoTen,
                Email = u.Email,
                CreatedAt = u.CreatedAt
            })
            .ToList();
    }
}
