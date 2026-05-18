using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;

public sealed record GetAnnouncementsQuery(
    Guid? NeighborhoodId = null,
    bool? IsActive = null) : IRequest<Result<IReadOnlyList<GetAnnouncementsQueryResponse>>>;
