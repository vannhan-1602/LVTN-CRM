namespace CRM.Application.Common.Models;

/// <summary>
/// Cấu hình cho LoyaltyService — thay cho việc bơm thẳng IConfiguration vào Application layer
/// (Application.csproj chỉ reference Microsoft.Extensions.Configuration.Abstractions, không có
/// bản đầy đủ để bind cấu hình; hơn nữa Application không nên phụ thuộc trực tiếp vào
/// IConfiguration với magic string key — dùng IOptions<LoyaltyOptions> strongly-typed sẽ an toàn
/// và rõ ràng hơn).
/// </summary>
public class LoyaltyOptions
{
    public const string SectionName = "Loyalty";

    /// <summary>
    /// URL gốc của frontend, dùng để build link redeem voucher gửi qua email
    /// (vd: thăng hạng, sinh nhật, ngày lễ...).
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5173";
}
