using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using SuggestionCommentEntity = UrbanCommunity.Domain.Entities.SuggestionComment;

namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.AddSuggestionComment;

public sealed class AddSuggestionCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddSuggestionCommentCommand, Result<AddSuggestionCommentCommandResponse>>
{
    public async Task<Result<AddSuggestionCommentCommandResponse>> Handle(AddSuggestionCommentCommand request, CancellationToken cancellationToken)
    {
        if (!await context.Suggestions.AsNoTracking().AnyAsync(suggestion => suggestion.Id == request.SuggestionId, cancellationToken))
        {
            throw new NotFoundException(nameof(Suggestion), request.SuggestionId);
        }

        if (!await context.Users.AsNoTracking().AnyAsync(user => user.Id == request.UserId && user.IsActive, cancellationToken))
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        var comment = new SuggestionCommentEntity
        {
            SuggestionId = request.SuggestionId,
            UserId = request.UserId,
            Content = request.Content.Trim()
        };

        context.SuggestionComments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        return Result<AddSuggestionCommentCommandResponse>.Success(new AddSuggestionCommentCommandResponse(
            comment.Id,
            comment.UserId,
            comment.Content,
            comment.CreatedAt));
    }
}
