using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Auth.DTOs;
using CRM.Application.Interfaces.Auth;
using CRM.Application.Models.Auth;
using MediatR;

namespace CRM.Application.Features.Auth.Commands.Refresh;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, RefreshResultDto>
{
    private const string ActiveStatus = "Active";

    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshCommandHandler(
        IRefreshTokenService refreshTokenService,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _refreshTokenService = refreshTokenService;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<RefreshResultDto> Handle(RefreshCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedException("Không tìm thấy phiên đăng nhập. Vui lòng đăng nhập lại.");
        }

        var rotateResult = await _refreshTokenService.RotateAsync(request.RefreshToken, request.RequestIp, ct);

        if (rotateResult.Status != RefreshTokenStatus.Success || rotateResult.UserId is null)
        {
            // Bao gồm cả trường hợp ReuseDetected: không tiết lộ lý do cụ thể ra ngoài,
            // luôn trả về cùng một thông điệp để tránh lộ thông tin cho kẻ tấn công.
            throw new UnauthorizedException("Phiên đăng nhập đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.");
        }

        var account = await _userRepository.GetByIdWithPasswordAsync(rotateResult.UserId.Value, ct)
            ?? throw new UnauthorizedException("Tài khoản không còn tồn tại.");

        if (!string.Equals(account.TrangThai, ActiveStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Tài khoản đang bị khóa hoặc chưa được kích hoạt.");
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

        return new RefreshResultDto
        {
            AccessToken = tokenResult.AccessToken,
            ExpiresAt = tokenResult.ExpiresAt,
            NewRefreshToken = rotateResult.NewPlainToken!,
            NewRefreshTokenExpiresAt = rotateResult.NewExpiresAt!.Value
        };
    }
}
