using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetManageableAnnouncements;

public sealed record GetManageableAnnouncementsQuery(
    Guid RequestingUserId,
    bool IsAdmin,
    string? Search = null,
    AnnouncementScope? Scope = null,
    bool? IsActive = null) : PaginationRequest, IRequest<Result<PagedResult<GetAnnouncementsQueryResponse>>>;
