using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed record GetMyNotificationsQuery(
    Guid UserId,
    bool? IsRead = null) : PaginationRequest, IRequest<Result<PagedResult<GetMyNotificationsQueryResponse>>>;
