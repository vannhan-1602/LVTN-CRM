using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

// Endpoint hỗ trợ TEST thủ công cho nghiệp vụ Loyalty (điểm/hạng/voucher/email tự động).
// Trong production, job này tự chạy hàng ngày qua LoyaltyDailyJobHostedService;
// endpoint dưới đây chỉ để kích hoạt ngay lập tức khi cần kiểm thử, không cần chờ tới giờ chạy.
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminOrManager)]
public class LoyaltyController : ControllerBase
{
    private readonly LoyaltyService _loyaltyService;
    public LoyaltyController(LoyaltyService loyaltyService) => _loyaltyService = loyaltyService;

    /// <summary>
    /// Chạy thủ công job hàng ngày: email sinh nhật/ngày thành lập, email ngày lễ,
    /// cảnh báo xuống hạng, và (nếu hôm nay là ngày 1) tính lại hạng toàn bộ KH.
    /// Dùng để test nhanh thay vì chờ hosted service tự chạy theo lịch.
    /// </summary>
    [HttpPost("run-daily-job")]
    public async Task<IActionResult> RunDailyJob(CancellationToken ct)
    {
        await _loyaltyService.ChayJobHangNgayAsync(ct);
        return Ok(ApiResponse.Ok("Đã chạy xong job loyalty hàng ngày. Kiểm tra bảng KH_EmailLog và KH_Voucher để xem kết quả."));
    }
}
