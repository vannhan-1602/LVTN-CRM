using CRM.Application.Features.Leads.Commands.CreatePublicLead;
using CRM.Application.Features.Leads.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.API.Controllers;

[Route("api/public/leads")]
[ApiController]
[AllowAnonymous]
[EnableCors("AllowPublicForms")]
[EnableRateLimiting("PublicFormSubmit")]
public class LeadPublicController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeadPublicController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateLeadFromLandingPage([FromBody] CreatePublicLeadRequestDto request, CancellationToken ct)
    {
        // Honeypot: bot điền vào field ẩn "Website" -> giả vờ thành công để không lộ cơ chế chặn cho bot,
        // nhưng không tạo lead thật.
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return Ok(new { success = true, message = "Gửi thông tin thành công! Chúng tôi sẽ liên hệ lại sớm." });
        }

        if (string.IsNullOrWhiteSpace(request.TenLead) || string.IsNullOrWhiteSpace(request.SoDienThoai))
        {
            return BadRequest(new { success = false, message = "Họ tên và Số điện thoại là bắt buộc." });
        }

        var command = new CreatePublicLeadCommand(
            request.TenLead,
            request.TenCongTy,
            request.SoDienThoai,
            request.Email
        );

        var leadId = await _mediator.Send(command, ct);

        return Ok(new { success = true, data = new { id = leadId }, message = "Gửi thông tin thành công! Chúng tôi sẽ liên hệ lại sớm." });
    }
}