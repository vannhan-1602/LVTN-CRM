using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Quotes.Commands.AcceptQuote;
using CRM.Application.Features.Quotes.Commands.CreateQuote;
using CRM.Application.Features.Quotes.Commands.DeleteQuote;
using CRM.Application.Features.Quotes.Commands.RejectQuote;
using CRM.Application.Features.Quotes.Commands.SendQuote;
using CRM.Application.Features.Quotes.Commands.UpdateQuote;
using CRM.Application.Features.Quotes.DTOs;
using CRM.Application.Features.Quotes.Queries.GetAllQuotes;
using CRM.Application.Features.Quotes.Queries.GetQuoteById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // policy cụ thể khai báo riêng từng action bên dưới 
public class QuoteController : ControllerBase
{
    private readonly IMediator _mediator;
    public QuoteController(IMediator mediator) => _mediator = mediator;

    // Đọc: Sale + Manager + Accountant (Accountant cần đối chiếu báo giá gốc khi xuất hóa đơn)
    [HttpGet]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? trangThai = null,
        [FromQuery] ulong? khachHangId = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllQuotesQuery(pageNumber, pageSize, search, trangThai, khachHangId), ct);
        return Ok(ApiResponse<PagedResult<QuoteDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetById(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetQuoteByIdQuery(id), ct);
        return Ok(ApiResponse<QuoteDetailDto>.Ok(result));
    }

    // Ghi: chỉ Sale + Manager (nghiệp vụ lập/gửi/xử lý báo giá thuộc về đội kinh doanh)
    [HttpPost]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Create([FromBody] CreateQuoteRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateQuoteCommand(request.KhachHangId, request.ChiTiet, request.MaVoucher), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<QuoteDetailDto>.Ok(result, "Tạo báo giá thành công."));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateQuoteRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateQuoteCommand(id, request.ChiTiet), ct);
        return Ok(ApiResponse<QuoteDetailDto>.Ok(result, "Cập nhật báo giá thành công."));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteQuoteCommand(id), ct);
        return Ok(ApiResponse.Ok("Xóa báo giá thành công."));
    }

    [HttpPost("{id:long}/send")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Send(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendQuoteCommand(id), ct);
        var message = result.EmailDaGui switch
        {
            true => "Đã gửi báo giá và email cho khách hàng.",
            false => $"Đã chuyển trạng thái báo giá sang Đã gửi, nhưng KHÔNG gửi được email: {result.EmailLyDoKhongGui}",
            null => "Đã gửi báo giá cho khách hàng.",
        };
        return Ok(ApiResponse<QuoteDto>.Ok(result, message));
    }

    [HttpPost("{id:long}/accept")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Accept(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new AcceptQuoteCommand(id), ct);
        return Ok(ApiResponse<QuoteDto>.Ok(result, "Báo giá đã được chấp nhận."));
    }

    [HttpPost("{id:long}/reject")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Reject(ulong id, [FromBody] RejectQuoteRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectQuoteCommand(id, request.LyDo), ct);
        return Ok(ApiResponse<QuoteDto>.Ok(result, "Đã từ chối báo giá."));
    }
}