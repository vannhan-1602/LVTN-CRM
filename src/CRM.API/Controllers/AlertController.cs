using CRM.Application.Common.Models;
using CRM.Application.Features.Alerts.DTOs;
using CRM.Application.Features.Alerts.Queries.GetDashboardAlerts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

// Cảnh báo tổng hợp cho Dashboard — mỗi vai trò (Admin/Manager/Sale/Accountant) thấy
// một tập cảnh báo phù hợp với phạm vi công việc của mình. Không gửi email nội bộ,
// chỉ hiển thị badge/thẻ cảnh báo; FE bấm vào 1 item sẽ điều hướng tới trang chi tiết
// tương ứng theo EntityType/EntityId.
[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertController : ControllerBase
{
    private readonly IMediator _mediator;
    public AlertController(IMediator mediator) => _mediator = mediator;

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardAlerts(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardAlertsQuery(), ct);
        return Ok(ApiResponse<DashboardAlertsDto>.Ok(result));
    }
}