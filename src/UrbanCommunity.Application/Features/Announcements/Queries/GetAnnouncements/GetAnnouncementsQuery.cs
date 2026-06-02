using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;

/// <summary>
/// Returns announcements accessible to the user based on their location.
/// The more specific the location, the more announcements are returned.
/// </summary>
/// <param name="ProvinceId">The user's province ID</param>
/// <param name="DistrictId">The user's district ID</param>
/// <param name="NeighborhoodId">The user's neighborhood ID (most specific)</param>
/// <param name="IsActive">Activity filter — defaults to active-only if not specified</param>
public sealed record GetAnnouncementsQuery(
    Guid? ProvinceId = null,
    Guid? DistrictId = null,
    Guid? NeighborhoodId = null,
    bool? IsActive = null) : IRequest<Result<IReadOnlyList<GetAnnouncementsQueryResponse>>>;
