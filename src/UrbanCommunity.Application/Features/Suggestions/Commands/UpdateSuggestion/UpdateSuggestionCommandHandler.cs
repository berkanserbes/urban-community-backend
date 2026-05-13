using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestion;

public sealed class UpdateSuggestionCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateSuggestionCommand, Result<UpdateSuggestionCommandResponse>>
{
    public async Task<Result<UpdateSuggestionCommandResponse>> Handle(UpdateSuggestionCommand request, CancellationToken cancellationToken)
    {
        var suggestion = await context.Suggestions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Suggestion), request.Id);

        if (suggestion.UserId != request.UserId)
        {
            throw new ForbiddenAccessException("Bu öneriyi düzenleme yetkiniz yok.");
        }

        if (suggestion.Status != SuggestionStatus.Active)
        {
            return Result<UpdateSuggestionCommandResponse>.Failure(["Yalnızca aktif öneriler düzenlenebilir."]);
        }

        suggestion.Title = request.Title.Trim();
        suggestion.Content = request.Content.Trim();

        await context.SaveChangesAsync(cancellationToken);

        return Result<UpdateSuggestionCommandResponse>.Success(
            new UpdateSuggestionCommandResponse(
                suggestion.Id,
                suggestion.Title,
                suggestion.Content,
                suggestion.UpdatedAt));
    }
}
