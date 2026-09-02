using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Auth.DTOs;
using CRM.Application.Interfaces.Auth;
using CRM.Application.Models.Auth;
using MediatR;

namespace CRM.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private const string ActiveStatus = "Active";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
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
            NhanSuId = account.NhanSuId,
            TokenVersion = account.TokenVersion
        };

        var tokenResult = _jwtTokenService.GenerateToken(authUser);
        var refreshTokenResult = await _refreshTokenService.IssueAsync(account.Id, request.RequestIp, cancellationToken);

        return new LoginResultDto
        {
            AccessToken = tokenResult.AccessToken,
            ExpiresAt = tokenResult.ExpiresAt,
            UserId = account.Id,
            Username = account.Username,
            Role = account.RoleName,
            HoTen = account.HoTen,
            Email = account.Email,
            NhanSuId = account.NhanSuId,
            RefreshToken = refreshTokenResult.PlainToken,
            RefreshTokenExpiresAt = refreshTokenResult.ExpiresAt
        };
    }
}