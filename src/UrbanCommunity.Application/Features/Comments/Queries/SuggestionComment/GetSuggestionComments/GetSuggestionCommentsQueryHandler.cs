using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using SuggestionEntity = UrbanCommunity.Domain.Entities.Suggestion;

namespace UrbanCommunity.Application.Features.Comments.Queries.SuggestionComment.GetSuggestionComments;

public sealed class GetSuggestionCommentsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuggestionCommentsQuery, Result<IReadOnlyList<GetSuggestionCommentsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetSuggestionCommentsQueryResponse>>> Handle(GetSuggestionCommentsQuery request, CancellationToken cancellationToken)
    {
        if (!await context.Suggestions.AsNoTracking().AnyAsync(suggestion => suggestion.Id == request.SuggestionId, cancellationToken))
        {
            throw new NotFoundException(nameof(SuggestionEntity), request.SuggestionId);
        }

        var comments = await context.SuggestionComments
            .AsNoTracking()
            .Where(comment => comment.SuggestionId == request.SuggestionId)
            .Include(comment => comment.User)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);

        IReadOnlyList<GetSuggestionCommentsQueryResponse> result = comments
            .Select(comment => new GetSuggestionCommentsQueryResponse(
                comment.Id,
                comment.UserId,
                comment.User?.FullName ?? "Silinmiş Kullanıcı",
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt))
            .ToList();

        return Result<IReadOnlyList<GetSuggestionCommentsQueryResponse>>.Success(result);
    }
}
