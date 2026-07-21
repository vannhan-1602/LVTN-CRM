using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Contracts.Commands.CreateContractFromQuote;
using CRM.Application.Features.Contracts.Commands.DeleteContract;
using CRM.Application.Features.Contracts.Commands.UpdateContractStatus;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Features.Contracts.Queries.GetAllContracts;
using CRM.Application.Features.Contracts.Queries.GetContractById;
using CRM.Application.Features.Contracts.Queries.GetLichThanhToanByHopDong;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractController : ControllerBase
{
    private readonly IMediator _mediator;
    public ContractController(IMediator mediator) => _mediator = mediator;

    //Accountant xem Customer và Contract chỉ đọc — dùng chung policy CustomerReadAccess đã có sẵn.
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
        var result = await _mediator.Send(new GetAllContractsQuery(pageNumber, pageSize, search, trangThai, khachHangId), ct);
        return Ok(ApiResponse<PagedResult<ContractDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetById(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractByIdQuery(id), ct);
        return Ok(ApiResponse<ContractDto>.Ok(result));
    }

    // Ghi: chỉ Sale + Manager (Accountant không tạo/sửa hợp đồng, chỉ xem)
    [HttpPost("from-quote")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> CreateFromQuote([FromBody] CreateContractFromQuoteRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateContractFromQuoteCommand(
                request.BaoGiaId, request.NgayKy, request.ThoiHan,
                request.HinhThucThanhToan, request.LichThanhToans), ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ContractDto>.Ok(result, "Tạo hợp đồng thành công."));
    }

    [HttpGet("{id:long}/lich-thanh-toan")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetLichThanhToan(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLichThanhToanByHopDongQuery(id), ct);
        return Ok(ApiResponse<List<Application.Features.Contracts.DTOs.LichThanhToanDto>>.Ok(result));
    }

    [HttpPut("{id:long}/status")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> UpdateStatus(ulong id, [FromBody] UpdateContractStatusRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateContractStatusCommand(id, request.TrangThai), ct);
        return Ok(ApiResponse<ContractDto>.Ok(result, "Cập nhật trạng thái hợp đồng thành công."));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteContractCommand(id), ct);
        return Ok(ApiResponse.Ok("Xóa hợp đồng thành công."));
    }
}