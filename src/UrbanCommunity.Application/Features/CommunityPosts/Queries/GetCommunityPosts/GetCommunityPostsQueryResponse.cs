using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Queries.GetCommunityPosts;

public sealed record GetCommunityPostsQueryResponse(
    Guid Id,
    PostType Type,
    string Title,
    string Content,
    CommunityPostStatus Status,
    Guid UserId,
    string UserFullName,
    CommunityPostScope Scope,
    Guid? ProvinceId,
    string? ProvinceName,
    Guid? DistrictId,
    string? DistrictName,
    Guid? NeighborhoodId,
    string? NeighborhoodName,
    int CommentCount,
    int MediaCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
