using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Users.Commands.CreateUser;
using CRM.Application.Features.Users.Commands.DeleteUser;
using CRM.Application.Features.Users.Commands.ResetPassword;
using CRM.Application.Features.Users.Commands.ToggleUserStatus;
using CRM.Application.Features.Users.Commands.UpdateUser;
using CRM.Application.Features.Users.DTOs;
using CRM.Application.Features.Users.Queries.GetAllUsers;
using CRM.Application.Features.Users.Queries.GetLookups;
using CRM.Application.Features.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;


// Quản trị tài khoản người dùng + thông tin nhân sự. Chỉ Admin được truy cập
// Role & Permission, Audit Log, thông tin nhân sự và phòng ban").

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminOnly)]
public class UserManagementController : ControllerBase
{
    private readonly IMediator _mediator;
    public UserManagementController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(), ct);
        return Ok(ApiResponse<List<UserDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(uint id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookups(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserLookupsQuery(), ct);
        return Ok(ApiResponse<UserLookupsDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateUserCommand(
            request.Username, request.Password, request.RoleId,
            request.HoTen, request.Email, request.SoDienThoai,
            request.PhongBanId, request.ChucVuId), ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<UserDto>.Ok(result, "Tạo tài khoản thành công."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(uint id, [FromBody] UpdateUserRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateUserCommand(
            id, request.RoleId, request.HoTen, request.Email,
            request.SoDienThoai, request.PhongBanId, request.ChucVuId), ct);

        return Ok(ApiResponse<UserDto>.Ok(result, "Cập nhật tài khoản thành công."));
    }

    [HttpPost("{id:long}/reset-password")]
    public async Task<IActionResult> ResetPassword(uint id, [FromBody] ResetPasswordRequestDto request, CancellationToken ct)
    {
        await _mediator.Send(new ResetPasswordCommand(id, request.NewPassword), ct);
        return Ok(ApiResponse.Ok("Đặt lại mật khẩu thành công."));
    }

    [HttpPost("{id:long}/status")]
    public async Task<IActionResult> ToggleStatus(uint id, [FromBody] ToggleUserStatusRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ToggleUserStatusCommand(id, request.TrangThai), ct);
        return Ok(ApiResponse<UserDto>.Ok(result, "Cập nhật trạng thái tài khoản thành công."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(uint id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteUserCommand(id), ct);
        return Ok(ApiResponse.Ok("Xóa tài khoản thành công."));
    }
}
