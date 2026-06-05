using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.CommunityPosts;
using UrbanCommunity.Application.Interfaces;
using CommunityPostCommentEntity = UrbanCommunity.Domain.Entities.CommunityPostComment;

namespace UrbanCommunity.Application.Features.Comments.Queries.CommunityPostComment.GetCommunityPostCommentById;

public sealed class GetCommunityPostCommentByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCommunityPostCommentByIdQuery, Result<GetCommunityPostCommentByIdQueryResponse>>
{
    public async Task<Result<GetCommunityPostCommentByIdQueryResponse>> Handle(GetCommunityPostCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await context.CommunityPostComments
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.CommunityPost)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPostCommentEntity), request.CommentId);

        if (!request.IsAdmin)
        {
            var accessScope = await CommunityPostAccessPolicy.ResolveUserAccessScopeAsync(context, request.RequestingUserId, cancellationToken);

            if (!comment.CommunityPost.IsVisibleTo(accessScope))
            {
                throw new ForbiddenAccessException("Bu topluluk gönderisi yorumunu görüntüleme yetkiniz yok.");
            }
        }

        return Result<GetCommunityPostCommentByIdQueryResponse>.Success(
            new GetCommunityPostCommentByIdQueryResponse(
                comment.Id,
                comment.UserId,
                comment.User.FullName,
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt));
    }
}
