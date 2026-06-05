using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Queries.GetAllowedCommunityPostStatusTransitions;

public sealed record GetAllowedCommunityPostStatusTransitionsQueryResponse(
    Guid CommunityPostId,
    CommunityPostStatus CurrentStatus,
    IReadOnlyList<CommunityPostStatus> AllowedNextStatuses);
