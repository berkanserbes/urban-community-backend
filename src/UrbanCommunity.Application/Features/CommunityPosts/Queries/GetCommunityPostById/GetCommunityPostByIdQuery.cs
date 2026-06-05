using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.CommunityPosts.Queries.GetCommunityPostById;

public sealed record GetCommunityPostByIdQuery(Guid Id, Guid RequestingUserId, bool IsAdmin = false)
    : IRequest<Result<GetCommunityPostByIdQueryResponse>>;
