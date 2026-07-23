using CRM.Application.Common.Constants;
using CRM.Application.Features.Auth.DTOs;
using CRM.Application.Interfaces.Auth;
using MediatR;

namespace CRM.Application.Features.Auth.Queries.GetStaffList;

//Danh sách nhân viên tối giản dùng cho dropdown "Nhân viên phụ trách / xử lý" ở
/// các module Customer/Lead/Opportunity/Ticket (và sắp tới là Mốc triển khai hợp
/// đồng). Chỉ Sale + Manager mới được là người phụ trách/xử lý — Accountant và
/// Admin không tham gia nghiệp vụ kinh doanh/chăm sóc nên bị loại khỏi danh sách.
/// Khác với GetUsersQuery (AdminOnly, đầy đủ thông tin tài khoản), query này chỉ
/// trả Id/HoTen/Role và mở cho mọi role đã đăng nhập.

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
            .Where(u => u.RoleName == Roles.Sale || u.RoleName == Roles.Manager)
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