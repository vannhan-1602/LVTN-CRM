using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Analytics.DTOs;
using CRM.Application.Features.Analytics.Queries.GenerateAiSalesAnalysis;
using CRM.Application.Features.Analytics.Queries.GetChiSummary;
using CRM.Application.Features.Analytics.Queries.GetDashboardTrends;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

// Tính năng AI phân tích dữ liệu bán hàng chỉ Manager được cấp quyền
//Endpoint tổng hợp số liệu doanh thu/cơ hội/kho/ticket/công nợ rồi gửi cho
// OpenAI để sinh nhận định + đề xuất hành động.

[ApiController]
[Route("api/analytics")]
[Authorize(Policy = Policies.ManagerOnly)]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    ///Phân tích bán hàng bằng AI trong N tháng gần nhất (mặc định 6, tối đa 24).
    [HttpGet("ai-sales-analysis")]
    public async Task<IActionResult> GetAiSalesAnalysis([FromQuery] int soThang = 6, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GenerateAiSalesAnalysisQuery(soThang), ct);
        return Ok(ApiResponse<AiSalesAnalysisResultDto>.Ok(result));
    }

    ///Số bản ghi mới tạo tháng này so với tháng trước — cho mũi tên xu hướng trên Dashboard.
    [HttpGet("dashboard-trends")]
    public async Task<IActionResult> GetDashboardTrends(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardTrendsQuery(), ct);
        return Ok(ApiResponse<DashboardTrendsDto>.Ok(result));
    }

    /// Tổng chi phí (Phiếu Chi) tháng này + top khách hàng phát sinh chi phí nhiều nhất.
    /// Không liên quan tới công nợ/tiến độ thanh toán — chỉ Manager xem được trên Dashboard.
    [HttpGet("chi-summary")]
    public async Task<IActionResult> GetChiSummary(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChiSummaryQuery(), ct);
        return Ok(ApiResponse<ChiSummaryDto>.Ok(result));
    }
}
