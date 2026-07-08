using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.AuditLog.DTOs;
using CRM.Application.Features.AuditLog.Queries.GetAuditLogs;
using CRM.Application.Features.AuditLog.Queries.GetAuditLogTableNames;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

// UC-QTV — Admin xem nhật ký hệ thống (SYS_AuditLog): ai đã INSERT/UPDATE/DELETE bản ghi nào,
// lúc nào, dữ liệu trước/sau ra sao. Chỉ Admin được xem (dữ liệu nhạy cảm toàn hệ thống).
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminOnly)]
public class AuditLogController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuditLogController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? tableName = null,
        [FromQuery] string? action = null,
        [FromQuery] uint? userId = null,
        [FromQuery] ulong? recordId = null,
        [FromQuery] DateTime? tuNgay = null,
        [FromQuery] DateTime? denNgay = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAuditLogsQuery(pageNumber, pageSize, tableName, action, userId, recordId, tuNgay, denNgay), ct);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
    }

    [HttpGet("table-names")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTableNames(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAuditLogTableNamesQuery(), ct);
        return Ok(ApiResponse<List<string>>.Ok(result));
    }
}
