using MediatR;
using Microsoft.AspNetCore.Http;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.UpdateCommunityPost;

public sealed record UpdateCommunityPostCommand(
    Guid Id,
    Guid RequestingUserId,
    string Title,
    string Content,
    IReadOnlyList<Guid>? KeepMediaIds = null,
    IReadOnlyList<IFormFile>? Media = null) : IRequest<Result<UpdateCommunityPostCommandResponse>>;
