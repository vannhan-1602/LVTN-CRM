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
            // Ghi nhận lần sai chỉ khi tài khoản có tồn tại (tránh lộ thông tin username có
            // tồn tại hay không qua timing, và tránh tạo "khóa" cho username không tồn tại).
            if (account is not null)
            {
                // 4b trước: nếu đã bị khóa tạm từ trước, không tính thêm lần sai mới, chỉ báo lại.
                if (account.KhoaDenThoiDiem.HasValue && account.KhoaDenThoiDiem.Value > DateTime.UtcNow)
                {
                    var conPhut = Math.Ceiling((account.KhoaDenThoiDiem.Value - DateTime.UtcNow).TotalMinutes);
                    throw new UnauthorizedException(
                        $"Tài khoản đang bị tạm khóa do nhập sai mật khẩu quá nhiều lần. Vui lòng thử lại sau {conPhut} phút.");
                }

                var soLanSai = await _userRepository.GhiNhanDangNhapSaiAsync(account.Id, cancellationToken);
                if (soLanSai >= 5)
                {
                    throw new UnauthorizedException(
                        "Tài khoản đã bị tạm khóa 15 phút do nhập sai mật khẩu quá 5 lần liên tiếp.");
                }
            }

            throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        // 4b. Tài khoản đang tạm khóa (dù mật khẩu lần này gõ đúng) — vẫn từ chối cho tới khi hết hạn khóa.
        if (account.KhoaDenThoiDiem.HasValue && account.KhoaDenThoiDiem.Value > DateTime.UtcNow)
        {
            var conPhut = Math.Ceiling((account.KhoaDenThoiDiem.Value - DateTime.UtcNow).TotalMinutes);
            throw new UnauthorizedException(
                $"Tài khoản đang bị tạm khóa do nhập sai mật khẩu quá nhiều lần. Vui lòng thử lại sau {conPhut} phút.");
        }

        if (!string.Equals(account.TrangThai, ActiveStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Tài khoản đang bị khóa hoặc chưa được kích hoạt.");
        }

        if (string.IsNullOrWhiteSpace(account.RoleName))
        {
            throw new ForbiddenException("Tài khoản chưa được gán vai trò truy cập.");
        }

        // Đăng nhập thành công: reset bộ đếm lần sai (nếu có) để không cộng dồn qua các lần sau.
        if (account.SoLanDangNhapSai > 0 || account.KhoaDenThoiDiem.HasValue)
        {
            await _userRepository.ResetDangNhapSaiAsync(account.Id, cancellationToken);
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