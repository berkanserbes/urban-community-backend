using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Notifications.Commands.DeleteAllNotifications;
using UrbanCommunity.Application.Features.Notifications.Commands.DeleteNotification;
using UrbanCommunity.Application.Features.Notifications.Commands.MarkAllAsRead;
using UrbanCommunity.Application.Features.Notifications.Commands.MarkAsRead;
using UrbanCommunity.Application.Features.Notifications.Commands.MarkAsUnread;
using UrbanCommunity.Application.Features.Notifications.Queries.GetNotificationById;
using UrbanCommunity.Application.Features.Notifications.Queries.GetMyNotifications;
using UrbanCommunity.Application.Features.Notifications.Queries.GetUnreadCount;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(IMediator mediator) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<GetMyNotificationsQueryResponse>>), 200)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] PaginationRequest pagination,
        [FromQuery] bool? isRead,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(
            new GetMyNotificationsQuery(userId, isRead)
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            },
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(new GetNotificationByIdQuery(id, userId, isAdmin), cancellationToken);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new GetUnreadNotificationCountQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/read")]
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

    [HttpPatch("{id:guid}/unread")]
    public async Task<IActionResult> MarkAsUnread(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new MarkNotificationAsUnreadCommand(id, userId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
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
