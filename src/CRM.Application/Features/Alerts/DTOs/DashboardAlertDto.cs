namespace CRM.Application.Features.Alerts.DTOs;

/// <summary>Mức độ nghiêm trọng để FE tô màu badge/thẻ cảnh báo.</summary>
public static class AlertSeverity
{
    public const string Info = "Info";
    public const string Warning = "Warning";
    public const string Danger = "Danger";
}

/// <summary>1 cảnh báo cụ thể, gắn với 1 bản ghi để FE điều hướng khi bấm vào.</summary>
public class DashboardAlertDto
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = AlertSeverity.Warning;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Lead | Customer | Ticket | Contract — FE dùng để build route điều hướng.</summary>
    public string EntityType { get; set; } = string.Empty;
    public ulong EntityId { get; set; }

    /// <summary>Mốc thời gian liên quan (hạn SLA, hạn thanh toán, giờ hẹn...) nếu có — dùng để sắp xếp/hiển thị.</summary>
    public DateTime? DueAt { get; set; }
}

/// <summary>1 nhóm cảnh báo cùng loại (VD: "Lead chưa phân công") — hiển thị thành 1 thẻ trên Dashboard.</summary>
public class DashboardAlertGroupDto
{
    public string GroupKey { get; set; } = string.Empty;
    public string GroupTitle { get; set; } = string.Empty;
    public string Severity { get; set; } = AlertSeverity.Warning;

    /// <summary>Tổng số bản ghi thỏa điều kiện — có thể lớn hơn số lượng trong Items (đã giới hạn để hiển thị gọn).</summary>
    public int Count { get; set; }
    public List<DashboardAlertDto> Items { get; set; } = new();
}

/// <summary>Toàn bộ cảnh báo Dashboard cho vai trò của người dùng hiện tại.</summary>
public class DashboardAlertsDto
{
    public int TongSoCanhBao { get; set; }
    public List<DashboardAlertGroupDto> Groups { get; set; } = new();
}
