using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Queries.GetCommunityPostById;

public sealed record GetCommunityPostByIdQueryResponse(
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
    IReadOnlyList<CommunityPostStatus> AllowedNextStatuses,
    IReadOnlyList<CommunityPostMediaDto> Media,
    int CommentCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CommunityPostMediaDto(
    Guid Id,
    string Url,
    string? ThumbnailUrl,
    string? ContentType,
    int DisplayOrder,
    MediaType MediaType);
