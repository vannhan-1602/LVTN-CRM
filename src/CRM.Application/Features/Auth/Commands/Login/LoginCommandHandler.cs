using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Auth.DTOs;
using CRM.Application.Interfaces.Auth;
using CRM.Application.Models.Auth;
using MediatR;

namespace CRM.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private const string ActiveStatus = "Active";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var account = await _userRepository.GetByUsernameWithRoleAsync(request.Username, cancellationToken);

        if (account is null || !_passwordHasher.Verify(request.Password, account.Password))
        {
            throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        if (!string.Equals(account.TrangThai, ActiveStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Tài khoản đang bị khóa hoặc chưa được kích hoạt.");
        }

        if (string.IsNullOrWhiteSpace(account.RoleName))
        {
            throw new ForbiddenException("Tài khoản chưa được gán vai trò truy cập.");
        }

        var authUser = new AuthUser
        {
            Id = account.Id,
            Username = account.Username,
            RoleName = account.RoleName,
            HoTen = account.HoTen,
            Email = account.Email,
            NhanSuId = account.NhanSuId   
        };

        var tokenResult = _jwtTokenService.GenerateToken(authUser);

        return new LoginResponseDto
        {
            AccessToken = tokenResult.AccessToken,
            ExpiresAt = tokenResult.ExpiresAt,
            UserId = account.Id,
            Username = account.Username,
            Role = account.RoleName,
            HoTen = account.HoTen,
            Email = account.Email,
            NhanSuId = account.NhanSuId   
        };
    }
}
