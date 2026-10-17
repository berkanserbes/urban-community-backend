using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetManageableAnnouncementStats;

public sealed record GetManageableAnnouncementStatsQuery(Guid RequestingUserId, bool IsAdmin)
    : IRequest<Result<GetManageableAnnouncementStatsQueryResponse>>;
