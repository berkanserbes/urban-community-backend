using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.ChangeSuggestionStatus;

public sealed class ChangeSuggestionStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ChangeSuggestionStatusCommand, Result<ChangeSuggestionStatusCommandResponse>>
{
    public async Task<Result<ChangeSuggestionStatusCommandResponse>> Handle(ChangeSuggestionStatusCommand request, CancellationToken cancellationToken)
    {
        var suggestion = await context.Suggestions
            .FirstOrDefaultAsync(s => s.Id == request.SuggestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Suggestion), request.SuggestionId);

        suggestion.Status = request.NewStatus;

        await context.SaveChangesAsync(cancellationToken);

        return Result<ChangeSuggestionStatusCommandResponse>.Success(
            new ChangeSuggestionStatusCommandResponse(
                suggestion.Id,
                suggestion.Title,
                suggestion.Status,
                $"Öneri durumu '{request.NewStatus}' olarak güncellendi."));
    }
}
