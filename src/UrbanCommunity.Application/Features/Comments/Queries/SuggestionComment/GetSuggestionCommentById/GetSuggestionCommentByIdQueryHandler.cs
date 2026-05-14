using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using SuggestionCommentEntity = UrbanCommunity.Domain.Entities.SuggestionComment;

namespace UrbanCommunity.Application.Features.Comments.Queries.SuggestionComment.GetSuggestionCommentById;

public sealed class GetSuggestionCommentByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuggestionCommentByIdQuery, Result<GetSuggestionCommentByIdQueryResponse>>
{
    public async Task<Result<GetSuggestionCommentByIdQueryResponse>> Handle(GetSuggestionCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await context.SuggestionComments
            .AsNoTracking()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(SuggestionCommentEntity), request.CommentId);

        return Result<GetSuggestionCommentByIdQueryResponse>.Success(
            new GetSuggestionCommentByIdQueryResponse(
                comment.Id,
                comment.UserId,
                comment.User.FullName,
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt));
    }
}
