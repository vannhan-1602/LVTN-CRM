using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Analytics.DTOs;
using CRM.Application.Features.Analytics.Queries.GenerateAiSalesAnalysis;
using CRM.Application.Features.Analytics.Queries.GetChiSummary;
using CRM.Application.Features.Analytics.Queries.GetDashboardTrends;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

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

    // Dữ liệu công ty-wide, không lọc theo user (đã xác nhận không có ICurrentUserService
    // trong các handler bên dưới) — an toàn để cache chung cho mọi Manager, không rủi ro lộ
    // dữ liệu chéo user. ai-sales-analysis đặc biệt đáng cache vì mỗi lần gọi tốn 1 lượt gọi
    // OpenAI (tiền + độ trễ); 2 phút TTL đủ để tránh spam khi user bấm F5 dashboard liên tục
    // mà vẫn đủ mới để không hiển thị số liệu quá cũ.

    ///Phân tích bán hàng bằng AI trong N tháng gần nhất (mặc định 6, tối đa 24).
    [HttpGet("ai-sales-analysis")]
    [OutputCache(PolicyName = "Dashboard")]
    public async Task<IActionResult> GetAiSalesAnalysis([FromQuery] int soThang = 6, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GenerateAiSalesAnalysisQuery(soThang), ct);
        return Ok(ApiResponse<AiSalesAnalysisResultDto>.Ok(result));
    }

    ///Số bản ghi mới tạo tháng này so với tháng trước — cho mũi tên xu hướng trên Dashboard.
    [HttpGet("dashboard-trends")]
    [OutputCache(PolicyName = "Dashboard")]
    public async Task<IActionResult> GetDashboardTrends(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardTrendsQuery(), ct);
        return Ok(ApiResponse<DashboardTrendsDto>.Ok(result));
    }

    /// Tổng chi phí (Phiếu Chi) tháng này + tổng/top khách hàng phát sinh chi phí nhiều nhất
    /// trong khoảng thời gian lọc (tuNgay/denNgay — bỏ trống = toàn thời gian).
    /// Không liên quan tới công nợ/tiến độ thanh toán — chỉ Manager xem được trên Dashboard.
    [HttpGet("chi-summary")]
    [OutputCache(PolicyName = "Dashboard")]
    public async Task<IActionResult> GetChiSummary(
        [FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChiSummaryQuery(tuNgay, denNgay), ct);
        return Ok(ApiResponse<ChiSummaryDto>.Ok(result));
    }
}