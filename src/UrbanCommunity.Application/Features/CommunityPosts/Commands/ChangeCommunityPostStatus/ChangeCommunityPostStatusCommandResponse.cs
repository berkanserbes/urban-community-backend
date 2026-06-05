using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.ChangeCommunityPostStatus;

public sealed record ChangeCommunityPostStatusCommandResponse(
    Guid CommunityPostId,
    CommunityPostStatus CurrentStatus,
    IReadOnlyList<CommunityPostStatus> AllowedNextStatuses);
