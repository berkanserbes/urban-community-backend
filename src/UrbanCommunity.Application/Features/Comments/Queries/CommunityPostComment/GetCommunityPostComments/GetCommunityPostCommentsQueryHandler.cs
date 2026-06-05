using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.CommunityPosts;
using UrbanCommunity.Application.Interfaces;
using CommunityPostEntity = UrbanCommunity.Domain.Entities.CommunityPost;

namespace UrbanCommunity.Application.Features.Comments.Queries.CommunityPostComment.GetCommunityPostComments;

public sealed class GetCommunityPostCommentsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCommunityPostCommentsQuery, Result<IReadOnlyList<GetCommunityPostCommentsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetCommunityPostCommentsQueryResponse>>> Handle(GetCommunityPostCommentsQuery request, CancellationToken cancellationToken)
    {
        var post = await context.CommunityPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(post => post.Id == request.CommunityPostId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPostEntity), request.CommunityPostId);

        if (!request.IsAdmin)
        {
            var accessScope = await CommunityPostAccessPolicy.ResolveUserAccessScopeAsync(context, request.RequestingUserId, cancellationToken);

            if (!post.IsVisibleTo(accessScope))
            {
                throw new ForbiddenAccessException("Bu topluluk gönderisinin yorumlarını görüntüleme yetkiniz yok.");
            }
        }

        var comments = await context.CommunityPostComments
            .AsNoTracking()
            .Where(comment => comment.CommunityPostId == request.CommunityPostId)
            .Include(comment => comment.User)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);

        IReadOnlyList<GetCommunityPostCommentsQueryResponse> result = comments
            .Select(comment => new GetCommunityPostCommentsQueryResponse(
                comment.Id,
                comment.UserId,
                comment.User.FullName,
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt))
            .ToList();

        return Result<IReadOnlyList<GetCommunityPostCommentsQueryResponse>>.Success(result);
    }
}
