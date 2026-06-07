using CRM.Application.Common.Models;
using CRM.Application.Features.Auth.Commands.Login;
using CRM.Application.Features.Auth.DTOs;
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
}
