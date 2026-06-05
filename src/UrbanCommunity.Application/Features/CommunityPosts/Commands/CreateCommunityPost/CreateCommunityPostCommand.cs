using MediatR;
using Microsoft.AspNetCore.Http;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.CreateCommunityPost;

public sealed record CreateCommunityPostCommand(
    PostType Type,
    string Title,
    string Content,
    Guid UserId,
    CommunityPostScope Scope = CommunityPostScope.Neighborhood,
    Guid? ProvinceId = null,
    Guid? DistrictId = null,
    Guid? NeighborhoodId = null,
    IReadOnlyList<IFormFile>? Media = null) : IRequest<Result<CreateCommunityPostCommandResponse>>;
