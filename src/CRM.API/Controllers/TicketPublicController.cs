using CRM.Application.Common.Models;
using CRM.Application.Features.Tickets.Commands.SubmitCsat;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Features.Tickets.Queries.GetCsatByToken;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

// Không đăng nhập — khách bấm link trong email khảo sát hài lòng (CSAT) và tự đánh giá.
// Xác thực bằng token ngẫu nhiên lưu trong TK_DanhGiaHaiLong.Token (unique),
// không phải bằng tài khoản CRM — vì vậy KHÔNG dùng [Authorize]. Tương tự QuotePublicController.
[ApiController]
[Route("api/public/tickets")]
[AllowAnonymous]
public class TicketPublicController : ControllerBase
{
    private readonly IMediator _mediator;
    public TicketPublicController(IMediator mediator) => _mediator = mediator;

    [HttpGet("csat/{token}")]
    public async Task<IActionResult> GetCsat(string token, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCsatByTokenQuery(token), ct);
        return Ok(ApiResponse<CsatDto>.Ok(result));
    }

    [HttpPost("csat/{token}")]
    public async Task<IActionResult> SubmitCsat(string token, [FromBody] SubmitCsatRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitCsatCommand(token, request.DiemDanhGia, request.NhanXet), ct);
        return Ok(ApiResponse<CsatDto>.Ok(result, "Cảm ơn quý khách đã đánh giá dịch vụ!"));
    }
}
