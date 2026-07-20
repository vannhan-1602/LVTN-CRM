using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

//nghiệp vụ Loyalty (điểm/hạng/voucher/email tự động).
//tự chạy hàng ngày qua LoyaltyDailyJobHostedService;
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminOrManager)]
public class LoyaltyController : ControllerBase
{
    private readonly LoyaltyService _loyaltyService;
    public LoyaltyController(LoyaltyService loyaltyService) => _loyaltyService = loyaltyService;

 
    // Chạy thủ công job hàng ngày: email sinh nhật/ngày thành lập, email ngày lễ,
    //cảnh báo xuống hạng, và (nếu hôm nay là ngày 1) tính lại hạng toàn bộ KH.
    // Quét TOÀN BỘ khách hàng đủ điều kiện trong hệ thống — không phải gửi cho 1 người.
    // Dùng để test nhanh thay vì chờ hosted service tự chạy theo lịch.
    [HttpPost("run-daily-job")]
    public async Task<IActionResult> RunDailyJob(CancellationToken ct)
    {
        var summary = await _loyaltyService.ChayJobHangNgayAsync(ct);
        return Ok(ApiResponse.Ok(summary));
    }
}
