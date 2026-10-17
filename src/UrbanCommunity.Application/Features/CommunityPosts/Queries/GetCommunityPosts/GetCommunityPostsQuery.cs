using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Queries.GetCommunityPosts;

public sealed record GetCommunityPostsQuery(
    Guid RequestingUserId,
    bool IsAdmin = false,
    CommunityPostScope? Scope = null,
    Guid? ProvinceId = null,
    Guid? DistrictId = null,
    Guid? NeighborhoodId = null,
    CommunityPostStatus? Status = null,
    PostType? Type = null,
    Guid? UserId = null) : PaginationRequest, IRequest<Result<PagedResult<GetCommunityPostsQueryResponse>>>;
