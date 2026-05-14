using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using CommunityPostCommentEntity = UrbanCommunity.Domain.Entities.CommunityPostComment;

namespace UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.DeleteCommunityPostComment;

public sealed class DeleteCommunityPostCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCommunityPostCommentCommand, Result<DeleteCommunityPostCommentCommandResponse>>
{
    public async Task<Result<DeleteCommunityPostCommentCommandResponse>> Handle(DeleteCommunityPostCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await context.CommunityPostComments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPostCommentEntity), request.CommentId);

        if (!request.IsAdmin && comment.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu yorumu yalnızca sahibi veya admin silebilir.");

        context.CommunityPostComments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);
        return Result<DeleteCommunityPostCommentCommandResponse>.Success(
            new DeleteCommunityPostCommentCommandResponse("Yorum başarıyla silindi."));
    }
}
