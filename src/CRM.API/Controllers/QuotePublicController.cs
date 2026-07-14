using CRM.Application.Common.Models;
using CRM.Application.Features.Quotes.Commands.AcceptQuote;
using CRM.Application.Features.Quotes.Commands.RejectQuote;
using CRM.Application.Features.Quotes.DTOs;
using CRM.Application.Features.Quotes.Queries.GetQuoteById;
using CRM.Application.Interfaces.Quotes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

/// <summary>
/// Endpoint công khai (không đăng nhập) để khách hàng bấm link trong email báo giá
/// và tự xem/chấp nhận/từ chối. Xác thực bằng token HMAC (xem IQuotePublicTokenService),
/// không phải bằng tài khoản CRM — vì vậy KHÔNG dùng [Authorize].
/// </summary>
[ApiController]
[Route("api/public/quotes")]
[AllowAnonymous]
public class QuotePublicController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IQuotePublicTokenService _tokenService;

    public QuotePublicController(IMediator mediator, IQuotePublicTokenService tokenService)
    {
        _mediator = mediator;
        _tokenService = tokenService;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetByToken(string token, CancellationToken ct)
    {
        var quoteId = _tokenService.ValidateToken(token);
        if (quoteId is null)
            return NotFound(ApiResponse.Fail("Liên kết không hợp lệ hoặc đã hết hạn."));

        var result = await _mediator.Send(new GetQuoteByIdQuery(quoteId.Value), ct);
        return Ok(ApiResponse<QuoteDetailDto>.Ok(result));
    }

    [HttpPost("{token}/accept")]
    public async Task<IActionResult> Accept(string token, CancellationToken ct)
    {
        var quoteId = _tokenService.ValidateToken(token);
        if (quoteId is null)
            return NotFound(ApiResponse.Fail("Liên kết không hợp lệ hoặc đã hết hạn."));

        var result = await _mediator.Send(new AcceptQuoteCommand(quoteId.Value), ct);
        return Ok(ApiResponse<QuoteDto>.Ok(result, "Bạn đã chấp nhận báo giá. Cảm ơn quý khách!"));
    }

    [HttpPost("{token}/reject")]
    public async Task<IActionResult> Reject(string token, [FromBody] RejectQuoteRequestDto request, CancellationToken ct)
    {
        var quoteId = _tokenService.ValidateToken(token);
        if (quoteId is null)
            return NotFound(ApiResponse.Fail("Liên kết không hợp lệ hoặc đã hết hạn."));

        var result = await _mediator.Send(new RejectQuoteCommand(quoteId.Value, request.LyDo), ct);
        return Ok(ApiResponse<QuoteDto>.Ok(result, "Bạn đã từ chối báo giá."));
    }
}
