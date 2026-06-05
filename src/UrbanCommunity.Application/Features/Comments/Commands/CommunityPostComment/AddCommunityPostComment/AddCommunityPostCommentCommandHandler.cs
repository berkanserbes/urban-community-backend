using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.CommunityPosts;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using CommunityPostCommentEntity = UrbanCommunity.Domain.Entities.CommunityPostComment;

namespace UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.AddCommunityPostComment;

public sealed class AddCommunityPostCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddCommunityPostCommentCommand, Result<AddCommunityPostCommentCommandResponse>>
{
    public async Task<Result<AddCommunityPostCommentCommandResponse>> Handle(AddCommunityPostCommentCommand request, CancellationToken cancellationToken)
    {
        var post = await context.CommunityPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(post => post.Id == request.CommunityPostId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPost), request.CommunityPostId);

        var accessScope = await CommunityPostAccessPolicy.ResolveUserAccessScopeAsync(context, request.UserId, cancellationToken);

        if (!post.IsVisibleTo(accessScope))
        {
            throw new ForbiddenAccessException("Bu topluluk gönderisine yorum yapma yetkiniz yok.");
        }

        var comment = new CommunityPostCommentEntity
        {
            CommunityPostId = request.CommunityPostId,
            UserId = request.UserId,
            Content = request.Content.Trim()
        };

        context.CommunityPostComments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        return Result<AddCommunityPostCommentCommandResponse>.Success(new AddCommunityPostCommentCommandResponse(
            comment.Id,
            comment.UserId,
            comment.Content,
            comment.CreatedAt));
    }
}
