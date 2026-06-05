using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.UpdateCommunityPost;

public sealed record UpdateCommunityPostCommandResponse(
    Guid Id,
    string Title,
    string Content,
    IReadOnlyList<UpdateCommunityPostMediaResponse> Media,
    DateTime? UpdatedAt);

public sealed record UpdateCommunityPostMediaResponse(
    Guid Id,
    string Url,
    string? OriginalFileName,
    string? ContentType,
    long? FileSizeBytes,
    int DisplayOrder,
    MediaType MediaType);
