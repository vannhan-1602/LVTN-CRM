using CRM.Application.Interfaces.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace CRM.API.Hubs;

public class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPublisher(IHubContext<NotificationHub> hubContext) => _hubContext = hubContext;

    public Task NotifyUserAsync(uint userId, string eventType, object payload, CancellationToken ct = default) =>
        _hubContext.Clients.Group($"user:{userId}").SendAsync(eventType, payload, ct);

    public Task NotifyRoleAsync(string roleName, string eventType, object payload, CancellationToken ct = default) =>
        _hubContext.Clients.Group($"role:{roleName}").SendAsync(eventType, payload, ct);
}