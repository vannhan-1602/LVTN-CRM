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



    [HttpPost("run-daily-job")]
    public async Task<IActionResult> RunDailyJob(CancellationToken ct, [FromQuery] ulong? khachHangId = null)
    {
        var summary = await _loyaltyService.ChayJobHangNgayAsync(ct, khachHangId);
        return Ok(ApiResponse.Ok(summary));
    }
}