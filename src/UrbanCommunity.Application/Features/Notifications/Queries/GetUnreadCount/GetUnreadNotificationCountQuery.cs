using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Notifications.Queries.GetUnreadCount;

public sealed record GetUnreadNotificationCountQuery(Guid UserId) : IRequest<Result<GetUnreadNotificationCountQueryResponse>>;
