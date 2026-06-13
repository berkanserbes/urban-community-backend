using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using SuggestionCommentEntity = UrbanCommunity.Domain.Entities.SuggestionComment;

namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.DeleteSuggestionComment;

public sealed class DeleteSuggestionCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteSuggestionCommentCommand, Result<DeleteSuggestionCommentCommandResponse>>
{
    public async Task<Result<DeleteSuggestionCommentCommandResponse>> Handle(DeleteSuggestionCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await context.SuggestionComments
            .Include(item => item.Suggestion)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(SuggestionCommentEntity), request.CommentId);

        if (!request.IsAdmin && comment.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu yorumu yalnızca sahibi veya admin silebilir.");

        if (!comment.Suggestion.AllowsCommentChanges())
        {
            return Result<DeleteSuggestionCommentCommandResponse>.Failure(
                ["Kapalı önerilerin yorumları silinemez."]);
        }

        context.SuggestionComments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);
        return Result<DeleteSuggestionCommentCommandResponse>.Success(
            new DeleteSuggestionCommentCommandResponse("Yorum başarıyla silindi."));
    }
}
