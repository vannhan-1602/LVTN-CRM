using CRM.Application.Features.Leads.Commands.CreatePublicLead;
using CRM.Application.Features.Leads.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.API.Controllers;

[Route("api/public/leads")]
[ApiController]
[AllowAnonymous]
public class LeadPublicController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeadPublicController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateLeadFromLandingPage([FromBody] CreatePublicLeadRequestDto request, CancellationToken ct)
    {
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