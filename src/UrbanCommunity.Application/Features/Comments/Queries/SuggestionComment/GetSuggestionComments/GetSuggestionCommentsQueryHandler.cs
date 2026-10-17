using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Extensions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Common.Projections;
using UrbanCommunity.Application.Interfaces;
using SuggestionEntity = UrbanCommunity.Domain.Entities.Suggestion;

namespace UrbanCommunity.Application.Features.Comments.Queries.SuggestionComment.GetSuggestionComments;

public sealed class GetSuggestionCommentsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuggestionCommentsQuery, Result<PagedResult<GetSuggestionCommentsQueryResponse>>>
{
    public async Task<Result<PagedResult<GetSuggestionCommentsQueryResponse>>> Handle(GetSuggestionCommentsQuery request, CancellationToken cancellationToken)
    {
        if (!await context.Suggestions.AsNoTracking().AnyAsync(suggestion => suggestion.Id == request.SuggestionId, cancellationToken))
        {
            throw new NotFoundException(nameof(SuggestionEntity), request.SuggestionId);
        }

        var comments = await context.SuggestionComments
            .AsNoTracking()
            .Where(comment => comment.SuggestionId == request.SuggestionId)
            .OrderBy(comment => comment.CreatedAt)
            .ThenBy(comment => comment.Id)
            .Select(comment => new GetSuggestionCommentsQueryResponse(
                comment.Id,
                comment.UserId,
                comment.User == null ? "Silinmiş Kullanıcı"
                    : UserProjections.FullName.Invoke(comment.User),
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt))
            .Expand()
            .ToPagedResultAsync(request.PageIndex, request.PageSize, cancellationToken);

        return Result<PagedResult<GetSuggestionCommentsQueryResponse>>.Success(comments);
    }
}
