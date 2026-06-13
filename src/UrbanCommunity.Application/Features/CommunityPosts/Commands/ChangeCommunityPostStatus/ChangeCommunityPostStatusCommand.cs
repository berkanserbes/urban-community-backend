using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.ChangeCommunityPostStatus;

public sealed record ChangeCommunityPostStatusCommand(
    Guid CommunityPostId,
    Guid RequestingUserId,
    bool IsAdmin,
    CommunityPostStatus NewStatus)
    : IRequest<Result<ChangeCommunityPostStatusCommandResponse>>;
