namespace UrbanCommunity.Application.Interfaces;

/// <summary>
/// Gerçek zamanlı bildirim için abstraction. SignalR implementasyonu API katmanında yapılır.
/// </summary>
public interface IRealtimeNotificationService
{
    Task SendToGroupAsync(string group, string eventName, object data, CancellationToken cancellationToken = default);
    Task SendToUserAsync(string userId, string eventName, object data, CancellationToken cancellationToken = default);
}
