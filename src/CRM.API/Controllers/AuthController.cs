using CRM.Application.Common.Models;
using CRM.Application.Features.Auth.Commands.ChangePassword;
using CRM.Application.Features.Auth.Commands.Login;
using CRM.Application.Features.Auth.Commands.Logout;
using CRM.Application.Features.Auth.Commands.Refresh;
using CRM.Application.Features.Auth.DTOs;
using CRM.Application.Features.Auth.Queries.GetStaffList;
using CRM.Application.Features.Auth.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CRM.Application.Common.Constants;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";

    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IMediator mediator, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginAttempt")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginCommand(request.Username, request.Password, GetClientIp()),
            cancellationToken);

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);

        // Không bao giờ trả RefreshToken trong body — chỉ map các field an toàn.
        var response = new LoginResponseDto
        {
            AccessToken = result.AccessToken,
            ExpiresAt = result.ExpiresAt,
            UserId = result.UserId,
            Username = result.Username,
            Role = result.Role,
            HoTen = result.HoTen,
            Email = result.Email,
            NhanSuId = result.NhanSuId
        };

        return Ok(ApiResponse<LoginResponseDto>.Ok(response, "Đăng nhập thành công."));
    }

    // Dùng refresh token trong cookie HttpOnly để lấy access token mới mà không cần đăng
    // nhập lại. Refresh token cũ bị revoke ngay (rotation) và cookie được thay bằng token mới.
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginAttempt")]
    [ProducesResponseType(typeof(ApiResponse<AccessTokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        try
        {
            var result = await _mediator.Send(
                new RefreshCommand(refreshToken ?? string.Empty, GetClientIp()),
                cancellationToken);

            SetRefreshTokenCookie(result.NewRefreshToken, result.NewRefreshTokenExpiresAt);

            var response = new AccessTokenResponseDto
            {
                AccessToken = result.AccessToken,
                ExpiresAt = result.ExpiresAt
            };

            return Ok(ApiResponse<AccessTokenResponseDto>.Ok(response, "Làm mới phiên đăng nhập thành công."));
        }
        catch (Exception)
        {
            // Bất kể lý do thất bại (hết hạn, không hợp lệ, hay reuse-detected), luôn xoá cookie
            // để buộc client quay lại màn hình đăng nhập thay vì lặp lại refresh vô ích.
            DeleteRefreshTokenCookie();
            throw;
        }
    }

    // Đăng xuất: thu hồi refresh token hiện tại và xoá cookie. Không thu hồi TokenVersion
    // (đăng xuất chỉ đóng phiên này, không ép mọi thiết bị khác đăng nhập lại).
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        await _mediator.Send(new LogoutCommand(refreshToken, GetClientIp()), cancellationToken);

        DeleteRefreshTokenCookie();

        return Ok(ApiResponse.Ok("Đăng xuất thành công."));
    }

    //Người dùng tự đổi mật khẩu của chính mình.
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ChangePasswordCommand(request.CurrentPassword, request.NewPassword, request.ConfirmNewPassword),
            cancellationToken);

        DeleteRefreshTokenCookie();

        return Ok(ApiResponse.Ok("Đổi mật khẩu thành công. Vui lòng đăng nhập lại."));
    }

    // Danh sách tài khoản đầy đủ — chỉ Admin 
    [HttpGet("users")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserSummaryDto>>.Ok(users));
    }

    
    // Danh sách nhân viên tối giản (Id/HoTen/Role) cho dropdown
    // "Nhân viên phụ trách / xử lý" ở Customer/Lead/Ticket. Mở cho mọi role
    // đã đăng nhập (không giới hạn AdminOnly như /users).
    [HttpGet("staff-list")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StaffLookupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaffList(CancellationToken cancellationToken)
    {
        var staff = await _mediator.Send(new GetStaffListQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StaffLookupDto>>.Ok(staff));
    }

    private void SetRefreshTokenCookie(string token, DateTime expiresAt)
    {
        // SameSite=None + Secure bắt buộc cho cross-site cookie (frontend Vercel <-> API domain
        // riêng) trên production/staging (luôn chạy HTTPS). Ở Development (localhost, thường
        // chạy HTTP), trình duyệt từ chối cookie Secure trên kết nối không mã hoá, nên dùng
        // SameSite=Lax + Secure=false — vẫn hoạt động vì hai cổng localhost cùng site.
        var isDev = _environment.IsDevelopment();

        Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = expiresAt,
            Path = "/api/auth"
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        var isDev = _environment.IsDevelopment();

        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/api/auth"
        });
    }

    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
