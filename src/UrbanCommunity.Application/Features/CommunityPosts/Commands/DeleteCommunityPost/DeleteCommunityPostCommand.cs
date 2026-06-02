using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.DeleteCommunityPost;

public sealed record DeleteCommunityPostCommand(Guid Id, Guid RequestingUserId, bool IsAdmin = false)
    : IRequest<Result<DeleteCommunityPostCommandResponse>>;
