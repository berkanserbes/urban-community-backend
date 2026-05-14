using Microsoft.AspNetCore.SignalR;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Api.Hubs;

namespace UrbanCommunity.Api.Services;

public sealed class SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    : IRealtimeNotificationService
{
    public Task SendToGroupAsync(string group, string eventName, object data, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(group).SendAsync(eventName, data, cancellationToken);

    public Task SendToUserAsync(string userId, string eventName, object data, CancellationToken cancellationToken = default)
        => hubContext.Clients.User(userId).SendAsync(eventName, data, cancellationToken);
}
