using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Analytics.DTOs;
using CRM.Application.Features.Analytics.Queries.GenerateAiSalesAnalysis;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

/// <summary>
/// Tính năng "AI phân tích dữ liệu bán hàng" — theo sơ đồ Use Case, chỉ Manager được cấp quyền
/// AI Analysis. Endpoint tổng hợp số liệu doanh thu/cơ hội/kho/ticket/công nợ rồi gửi cho
/// OpenAI để sinh nhận định + đề xuất hành động.
/// </summary>
[ApiController]
[Route("api/analytics")]
[Authorize(Policy = Policies.ManagerOnly)]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Phân tích bán hàng bằng AI trong N tháng gần nhất (mặc định 6, tối đa 24).</summary>
    [HttpGet("ai-sales-analysis")]
    public async Task<IActionResult> GetAiSalesAnalysis([FromQuery] int soThang = 6, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GenerateAiSalesAnalysisQuery(soThang), ct);
        return Ok(ApiResponse<AiSalesAnalysisResultDto>.Ok(result));
    }
}
