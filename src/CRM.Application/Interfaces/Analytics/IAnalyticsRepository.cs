using CRM.Application.Features.Analytics.DTOs;

namespace CRM.Application.Interfaces.Analytics;

public interface IAnalyticsRepository
{
    /// <summary>
    /// Gộp số liệu bán hàng trong N tháng gần nhất: doanh thu theo tháng, tỉ lệ thắng cơ hội,
    /// top sản phẩm bán chạy, tình hình ticket, công nợ chưa thu. Dùng làm dữ liệu đầu vào
    /// cho tính năng "AI phân tích dữ liệu bán hàng" (Manager) — KHÔNG chứa văn bản phân tích,
    /// chỉ số liệu thô; phần diễn giải do AI sinh ra ở lớp trên.
    /// </summary>
    Task<SalesAnalyticsDataDto> GetSalesAnalyticsDataAsync(int soThang, CancellationToken ct = default);

    /// <summary>So sánh số khách hàng/hợp đồng/báo giá/ticket mới tạo tháng này với tháng trước —
    /// dùng cho mũi tên xu hướng (tăng/giảm) trên 4 stat card đầu Dashboard.</summary>
    Task<DashboardTrendsDto> GetDashboardTrendsAsync(CancellationToken ct = default);

    /// <summary>Tổng chi phí (Phiếu Chi) tháng này + top khách hàng phát sinh chi phí nhiều nhất
    /// (toàn thời gian). Không liên quan tới công nợ/tiến độ thanh toán.</summary>
    Task<ChiSummaryDto> GetChiSummaryAsync(CancellationToken ct = default);
}
