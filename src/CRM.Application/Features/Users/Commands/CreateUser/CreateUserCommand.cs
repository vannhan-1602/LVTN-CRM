using CRM.Application.Features.Users.DTOs;
using MediatR;

namespace CRM.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Username,
    string Password,
    uint RoleId,
    string HoTen,
    string? Email,
    string? SoDienThoai,
    ushort? PhongBanId,
    ushort? ChucVuId
) : IRequest<UserDto>;
