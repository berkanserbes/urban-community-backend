using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.CommunityPosts.Queries.GetAllowedCommunityPostStatusTransitions;

public sealed record GetAllowedCommunityPostStatusTransitionsQuery(Guid CommunityPostId, Guid RequestingUserId, bool IsAdmin = false)
    : IRequest<Result<GetAllowedCommunityPostStatusTransitionsQueryResponse>>;
