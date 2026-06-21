using CRM.Application.Common.Models;
using CRM.Application.Features.Auth.Commands.Login;
using CRM.Application.Features.Auth.DTOs;
using CRM.Application.Features.Auth.Queries.GetStaffList;
using CRM.Application.Features.Auth.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Application.Common.Constants;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginCommand(request.Username, request.Password),
            cancellationToken);

        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Đăng nhập thành công."));
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
}
