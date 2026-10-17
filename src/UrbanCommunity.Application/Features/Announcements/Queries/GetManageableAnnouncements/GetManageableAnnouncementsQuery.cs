using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetManageableAnnouncements;

public sealed record GetManageableAnnouncementsQuery(
    Guid RequestingUserId,
    bool IsAdmin) : PaginationRequest, IRequest<Result<PagedResult<GetAnnouncementsQueryResponse>>>;
