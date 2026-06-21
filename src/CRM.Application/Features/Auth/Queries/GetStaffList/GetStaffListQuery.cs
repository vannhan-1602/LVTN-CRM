using CRM.Application.Features.Auth.DTOs;
using CRM.Application.Interfaces.Auth;
using MediatR;

namespace CRM.Application.Features.Auth.Queries.GetStaffList;

//Danh sách nhân viên tối giản dùng cho dropdown "Nhân viên phụ trách / xử lý" ở
/// các module Customer/Lead/Ticket. Khác với GetUsersQuery (AdminOnly, đầy đủ thông
/// tin tài khoản), query này chỉ trả Id/HoTen/Role và mở cho mọi role đã đăng nhập.

public record GetStaffListQuery : IRequest<IReadOnlyList<StaffLookupDto>>;

public class GetStaffListQueryHandler : IRequestHandler<GetStaffListQuery, IReadOnlyList<StaffLookupDto>>
{
    private readonly IUserRepository _userRepository;
    public GetStaffListQueryHandler(IUserRepository userRepository) => _userRepository = userRepository;

    public async Task<IReadOnlyList<StaffLookupDto>> Handle(GetStaffListQuery request, CancellationToken ct)
    {
        var users = await _userRepository.GetAllWithRolesAsync(ct);

        return users
            .Where(u => !string.IsNullOrWhiteSpace(u.HoTen))
            .Select(u => new StaffLookupDto
            {
                Id = u.Id,
                HoTen = u.HoTen!,
                Role = u.RoleName
            })
            .OrderBy(u => u.HoTen)
            .ToList();
    }
}
