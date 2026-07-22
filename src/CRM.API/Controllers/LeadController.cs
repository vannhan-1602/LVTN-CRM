using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Leads.Commands.ConvertLead;
using CRM.Application.Features.Leads.Commands.CreateLead;
using CRM.Application.Features.Leads.Commands.DeleteLead;
using CRM.Application.Features.Leads.Commands.RestoreLead;
using CRM.Application.Features.Leads.Commands.UpdateLead;
using CRM.Application.Features.Leads.DTOs;
using CRM.Application.Features.Leads.Queries.GetAllLeads;
using CRM.Application.Features.Leads.Queries.GetLeadById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.SalesTeam)]   // Sale + Manager
    public class LeadController : ControllerBase
    {
        private readonly IMediator _mediator;
        public LeadController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isDeleted = null,
            [FromQuery] string? tinhTrang = null,
            CancellationToken ct = default)
        {
            var result = await _mediator.Send(new GetAllLeadsQuery(pageNumber, pageSize, search, isDeleted, tinhTrang), ct);
            return Ok(ApiResponse<PagedResult<LeadDto>>.Ok(result));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(ulong id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetLeadByIdQuery(id), ct);
            return Ok(ApiResponse<LeadDto>.Ok(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeadRequestDto request, CancellationToken ct)
        {
            var result = await _mediator.Send(
                new CreateLeadCommand(request.TenLead, request.TenCongTy,
                    request.SoDienThoai, request.Email, request.NhanVienPhuTrachId), ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<LeadDto>.Ok(result, "Tạo lead thành công."));
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(ulong id, [FromBody] UpdateLeadRequestDto request, CancellationToken ct)
        {
            var result = await _mediator.Send(
                new UpdateLeadCommand(id, request.TenLead, request.TenCongTy,
                    request.SoDienThoai, request.Email, request.TinhTrang, request.NhanVienPhuTrachId), ct);
            return Ok(ApiResponse<LeadDto>.Ok(result, "Cập nhật lead thành công."));
        }

        // ManagerOnly 
        [HttpDelete("{id:long}")]
        [Authorize(Policy = Policies.ManagerOnly)]
        public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
        {
            await _mediator.Send(new DeleteLeadCommand(id), ct);
            return Ok(ApiResponse.Ok("Xóa lead thành công."));
        }

        // ManagerOnly — khôi phục lead đã bị khóa/xóa mềm
        [HttpPost("{id:long}/restore")]
        [Authorize(Policy = Policies.ManagerOnly)]
        public async Task<IActionResult> Restore(ulong id, CancellationToken ct)
        {
            await _mediator.Send(new RestoreLeadCommand(id), ct);
            return Ok(ApiResponse.Ok("Khôi phục lead thành công."));
        }

        // Convert của Sale "chuyển đổi lead thành khách hàng"
        // nằm trong quyền hạn chính của Sale
        [HttpPost("{id:long}/convert")]
        public async Task<IActionResult> Convert(ulong id, [FromBody] ConvertLeadCommand request, CancellationToken ct)
        {
            var result = await _mediator.Send(request with { LeadId = id }, ct);
            return Ok(ApiResponse<CustomerDto>.Ok(result, "Chuyển đổi lead thành khách hàng thành công."));
        }
    }
}