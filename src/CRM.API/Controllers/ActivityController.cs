using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Activities.Commands.CreateActivity;
using CRM.Application.Features.Activities.Commands.DeleteActivity;
using CRM.Application.Features.Activities.Commands.UpdateActivity;
using CRM.Application.Features.Activities.DTOs;
using CRM.Application.Features.Activities.Queries.GetActivitiesByCustomer;
using CRM.Application.Features.Activities.Queries.GetActivitiesByLead;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

/// <summary>
/// Nhật ký hoạt động chăm sóc khách hàng / lead (KH_HoatDong).
/// Sale + Manager: ghi nhận cuộc gọi, cuộc họp, email, zalo với KH hoặc Lead.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.SalesTeam)]
public class ActivityController : ControllerBase
{
    private readonly IMediator _mediator;
    public ActivityController(IMediator mediator) => _mediator = mediator;

    [HttpGet("customer/{khachHangId:long}")]
    public async Task<IActionResult> GetByCustomer(ulong khachHangId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActivitiesByCustomerQuery(khachHangId), ct);
        return Ok(ApiResponse<List<ActivityDto>>.Ok(result));
    }

    [HttpGet("lead/{leadId:long}")]
    public async Task<IActionResult> GetByLead(ulong leadId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActivitiesByLeadQuery(leadId), ct);
        return Ok(ApiResponse<List<ActivityDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActivityRequestDto req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateActivityCommand(req.KhachHangId, req.LeadId, req.LoaiHoatDong, req.NoiDung, req.ThoiGianThucHien), ct);
        return Ok(ApiResponse<ActivityDto>.Ok(result, "Ghi nhận hoạt động thành công."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateActivityRequestDto req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateActivityCommand(id, req.LoaiHoatDong, req.NoiDung, req.ThoiGianThucHien), ct);
        return Ok(ApiResponse<ActivityDto>.Ok(result, "Cập nhật hoạt động thành công."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteActivityCommand(id), ct);
        return Ok(ApiResponse.Ok("Xóa hoạt động thành công."));
    }
}