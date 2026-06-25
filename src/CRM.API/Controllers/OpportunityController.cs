using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Opportunities.Commands.ChangeOpportunityStage;
using CRM.Application.Features.Opportunities.Commands.CreateOpportunity;
using CRM.Application.Features.Opportunities.Commands.DeleteOpportunity;
using CRM.Application.Features.Opportunities.Commands.UpdateOpportunity;
using CRM.Application.Features.Opportunities.DTOs;
using CRM.Application.Features.Opportunities.Queries.GetAllOpportunities;
using CRM.Application.Features.Opportunities.Queries.GetOpportunityById;
using CRM.Application.Features.Opportunities.Queries.GetOpportunitySummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.SalesTeam)]  // Sale + Manager
public class OpportunityController : ControllerBase
{
    private readonly IMediator _mediator;
    public OpportunityController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? giaiDoan = null,
        [FromQuery] ulong? khachHangId = null,
        [FromQuery] ulong? leadId = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAllOpportunitiesQuery(pageNumber, pageSize, search, giaiDoan, khachHangId, leadId), ct);
        return Ok(ApiResponse<PagedResult<OpportunityDto>>.Ok(result));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOpportunitySummaryQuery(), ct);
        return Ok(ApiResponse<OpportunitySummaryDto>.Ok(result));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOpportunityByIdQuery(id), ct);
        return Ok(ApiResponse<OpportunityDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityRequestDto req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateOpportunityCommand(
            req.TenThuongVu, req.KhachHangId, req.LeadId,
            req.TyLeThanhCong, req.DoanhThuKyVong, req.GhiChu, req.NgayDuKien), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<OpportunityDto>.Ok(result, "Tạo cơ hội bán hàng thành công."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateOpportunityRequestDto req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateOpportunityCommand(
            id, req.TenThuongVu, req.KhachHangId, req.LeadId,
            req.TyLeThanhCong, req.DoanhThuKyVong, req.GhiChu, req.NgayDuKien), ct);
        return Ok(ApiResponse<OpportunityDto>.Ok(result, "Cập nhật thành công."));
    }

    [HttpPost("{id:long}/stage")]
    public async Task<IActionResult> ChangeStage(ulong id, [FromBody] ChangeStageRequestDto req, CancellationToken ct)
    {
        var result = await _mediator.Send(new ChangeOpportunityStageCommand(id, req.GiaiDoan, req.GhiChu), ct);
        return Ok(ApiResponse<OpportunityDto>.Ok(result, "Cập nhật giai đoạn thành công."));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteOpportunityCommand(id), ct);
        return Ok(ApiResponse.Ok("Xóa cơ hội bán hàng thành công."));
    }
}