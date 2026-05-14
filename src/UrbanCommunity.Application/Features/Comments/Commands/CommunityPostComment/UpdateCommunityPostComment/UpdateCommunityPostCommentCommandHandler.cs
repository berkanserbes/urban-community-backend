using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using CommunityPostCommentEntity = UrbanCommunity.Domain.Entities.CommunityPostComment;

namespace UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.UpdateCommunityPostComment;

public sealed class UpdateCommunityPostCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCommunityPostCommentCommand, Result<UpdateCommunityPostCommentCommandResponse>>
{
    public async Task<Result<UpdateCommunityPostCommentCommandResponse>> Handle(UpdateCommunityPostCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await context.CommunityPostComments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPostComment), request.CommentId);

        if (comment.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu yorumu yalnızca sahibi düzenleyebilir.");

        comment.Content = request.Content.Trim();

        await context.SaveChangesAsync(cancellationToken);
        return Result<UpdateCommunityPostCommentCommandResponse>.Success(
            new UpdateCommunityPostCommentCommandResponse(comment.Id, comment.Content, comment.UpdatedAt));
    }
}
