using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using SuggestionCommentEntity = UrbanCommunity.Domain.Entities.SuggestionComment;

namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.UpdateSuggestionComment;

public sealed class UpdateSuggestionCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateSuggestionCommentCommand, Result<UpdateSuggestionCommentCommandResponse>>
{
    public async Task<Result<UpdateSuggestionCommentCommandResponse>> Handle(UpdateSuggestionCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await context.SuggestionComments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(SuggestionComment), request.CommentId);

        if (comment.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu yorumu yalnızca sahibi düzenleyebilir.");

        comment.Content = request.Content.Trim();

        await context.SaveChangesAsync(cancellationToken);
        return Result<UpdateSuggestionCommentCommandResponse>.Success(
            new UpdateSuggestionCommentCommandResponse(comment.Id, comment.Content, comment.UpdatedAt));
    }
}
