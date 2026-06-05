using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.CommunityPosts;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.CommunityPosts.Queries.GetAllowedCommunityPostStatusTransitions;

public sealed class GetAllowedCommunityPostStatusTransitionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllowedCommunityPostStatusTransitionsQuery, Result<GetAllowedCommunityPostStatusTransitionsQueryResponse>>
{
    public async Task<Result<GetAllowedCommunityPostStatusTransitionsQueryResponse>> Handle(
        GetAllowedCommunityPostStatusTransitionsQuery request,
        CancellationToken cancellationToken)
    {
        var post = await context.CommunityPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.CommunityPostId, cancellationToken)
            ?? throw new NotFoundException("CommunityPost", request.CommunityPostId);

        if (!request.IsAdmin)
        {
            var accessScope = await CommunityPostAccessPolicy.ResolveUserAccessScopeAsync(context, request.RequestingUserId, cancellationToken);

            if (!post.IsVisibleTo(accessScope))
            {
                throw new ForbiddenAccessException("Bu topluluk gönderisinin durum geçişlerini görüntüleme yetkiniz yok.");
            }
        }

        return Result<GetAllowedCommunityPostStatusTransitionsQueryResponse>.Success(
            new GetAllowedCommunityPostStatusTransitionsQueryResponse(
                post.Id,
                post.Status,
                post.GetAllowedNextStatuses().ToList()));
    }
}
