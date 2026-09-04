namespace CRM.Application.Interfaces.Notifications;


public interface INotificationPublisher
{
    /// Gửi cho đúng 1 người dùng (VD: "ticket vừa được giao cho bạn").
    Task NotifyUserAsync(uint userId, string eventType, object payload, CancellationToken ct = default);

    /// Gửi cho mọi người dùng đang có 1 vai trò cụ thể (VD: "role:Sale" khi có Lead mới).
    Task NotifyRoleAsync(string roleName, string eventType, object payload, CancellationToken ct = default);
}