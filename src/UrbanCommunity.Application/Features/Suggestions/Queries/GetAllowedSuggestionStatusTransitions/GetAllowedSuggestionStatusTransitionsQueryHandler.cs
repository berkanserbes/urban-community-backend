using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetAllowedSuggestionStatusTransitions;

public sealed class GetAllowedSuggestionStatusTransitionsQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<
        GetAllowedSuggestionStatusTransitionsQuery,
        Result<GetAllowedSuggestionStatusTransitionsQueryResponse>>
{
    public async Task<Result<GetAllowedSuggestionStatusTransitionsQueryResponse>> Handle(
        GetAllowedSuggestionStatusTransitionsQuery request,
        CancellationToken cancellationToken)
    {
        var actorRole = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == request.RequestingUserId && user.IsActive)
            .Select(user => (UserRole?)user.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (actorRole is not (UserRole.Admin or UserRole.Official))
        {
            throw new ForbiddenAccessException(
                "Öneri durum geçişlerini görüntülemek için admin veya official yetkisi gereklidir.");
        }

        var suggestion = await context.Suggestions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == request.SuggestionId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(Suggestion), request.SuggestionId);

        return Result<GetAllowedSuggestionStatusTransitionsQueryResponse>.Success(
            new GetAllowedSuggestionStatusTransitionsQueryResponse(
                suggestion.Id,
                suggestion.Status,
                suggestion.GetAllowedNextStatuses().ToList()));
    }
}
