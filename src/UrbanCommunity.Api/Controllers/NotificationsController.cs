using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.Notifications.Commands.DeleteAllNotifications;
using UrbanCommunity.Application.Features.Notifications.Commands.DeleteNotification;
using UrbanCommunity.Application.Features.Notifications.Commands.MarkAllAsRead;
using UrbanCommunity.Application.Features.Notifications.Commands.MarkAsRead;
using UrbanCommunity.Application.Features.Notifications.Commands.MarkAsUnread;
using UrbanCommunity.Application.Features.Notifications.Queries.GetMyNotifications;
using UrbanCommunity.Application.Features.Notifications.Queries.GetUnreadCount;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(IMediator mediator) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool? isRead,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new GetMyNotificationsQuery(userId, isRead), cancellationToken);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new GetUnreadNotificationCountQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new MarkNotificationAsReadCommand(id, userId), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new MarkAllNotificationsAsReadCommand(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id}/unread")]
    public async Task<IActionResult> MarkAsUnread(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new MarkNotificationAsUnreadCommand(id, userId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new DeleteNotificationCommand(id, userId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAll(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new DeleteAllNotificationsCommand(userId), cancellationToken);
        return Ok(result);
    }

}
