using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace UrbanCommunity.Api.Hubs;

/// <summary>
/// SignalR Hub — Anlık bildirimler için.
/// Client'lar kapsamlarına göre gruplara katılır ve o gruba ait bildirimleri anlık alır.
/// 
/// Grup yapısı:
///   - "all"                    → Tüm kullanıcılar
///   - "province_{id}"          → İl bazında
///   - "district_{id}"          → İlçe bazında
///   - "neighborhood_{id}"      → Mahalle bazında
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    public async Task JoinAll()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "all");
    }

    public async Task LeaveAll()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all");
    }

    public async Task JoinProvince(string provinceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"province_{provinceId}");
    }

    public async Task LeaveProvince(string provinceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"province_{provinceId}");
    }

    public async Task JoinDistrict(string districtId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"district_{districtId}");
    }

    public async Task LeaveDistrict(string districtId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"district_{districtId}");
    }

    public async Task JoinNeighborhood(string neighborhoodId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"neighborhood_{neighborhoodId}");
    }

    public async Task LeaveNeighborhood(string neighborhoodId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"neighborhood_{neighborhoodId}");
    }
}