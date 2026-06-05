using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.CreateCommunityPost;

public sealed record CreateCommunityPostCommandResponse(
    Guid Id,
    string Type,
    string Title,
    string Status,
    CommunityPostScope Scope,
    Guid? ProvinceId,
    Guid? DistrictId,
    Guid? NeighborhoodId,
    IReadOnlyList<CreateCommunityPostMediaResponse> Media,
    DateTime CreatedAt);

public sealed record CreateCommunityPostMediaResponse(
    Guid Id,
    string Url,
    string? OriginalFileName,
    string? ContentType,
    long? FileSizeBytes,
    int DisplayOrder,
    MediaType MediaType);
