using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Notifications.Queries.GetNotificationById;

public sealed record GetNotificationByIdQuery(Guid NotificationId, Guid UserId, bool IsAdmin = false)
    : IRequest<Result<GetNotificationByIdQueryResponse>>;
