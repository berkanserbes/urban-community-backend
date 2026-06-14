using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Application.Realtime;

namespace UrbanCommunity.Api.Hubs;

/// <summary>
/// Assigns authenticated users to notification groups from their current
/// database location. Clients cannot choose their own location groups.
/// </summary>
[Authorize]
public sealed class NotificationHub(
    IApplicationDbContext context,
    ILogger<NotificationHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            RejectConnection("SignalR baglantisinda gecerli kullanici kimligi bulunamadi.");
            return;
        }

        var location = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => new UserLocation(
                user.NeighborhoodId,
                user.Neighborhood != null
                    ? user.Neighborhood.DistrictId
                    : null,
                user.Neighborhood != null
                    ? user.Neighborhood.DistrictNavigation.ProvinceId
                    : null))
            .SingleOrDefaultAsync(Context.ConnectionAborted);

        if (location is null)
        {
            RejectConnection("SignalR baglantisi icin aktif kullanici kaydi bulunamadi.", userId);
            return;
        }

        var groupNames = new List<string>
        {
            NotificationGroupNames.All
        };

        if (location.NeighborhoodId.HasValue &&
            location.DistrictId.HasValue &&
            location.ProvinceId.HasValue)
        {
            groupNames.Add(NotificationGroupNames.Province(location.ProvinceId.Value));
            groupNames.Add(NotificationGroupNames.District(location.DistrictId.Value));
            groupNames.Add(NotificationGroupNames.Neighborhood(location.NeighborhoodId.Value));
        }

        await Task.WhenAll(groupNames.Select(groupName =>
            Groups.AddToGroupAsync(
                Context.ConnectionId,
                groupName,
                Context.ConnectionAborted)));

        await base.OnConnectedAsync();
    }

    private void RejectConnection(string message, Guid? userId = null)
    {
        logger.LogWarning(
            "{Message} UserId: {UserId}, ConnectionId: {ConnectionId}",
            message,
            userId,
            Context.ConnectionId);

        Context.Abort();
    }

    private sealed record UserLocation(
        Guid? NeighborhoodId,
        Guid? DistrictId,
        Guid? ProvinceId);
}
