using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.DeleteSuggestion;

public sealed class DeleteSuggestionCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteSuggestionCommand, Result<DeleteSuggestionCommandResponse>>
{
    public async Task<Result<DeleteSuggestionCommandResponse>> Handle(DeleteSuggestionCommand request, CancellationToken cancellationToken)
    {
        var suggestion = await context.Suggestions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Suggestion), request.Id);

        if (!request.IsAdmin && suggestion.UserId != request.UserId)
        {
            throw new ForbiddenAccessException("Bu öneriyi silme yetkiniz yok.");
        }

        context.Suggestions.Remove(suggestion);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteSuggestionCommandResponse>.Success(
            new DeleteSuggestionCommandResponse("Öneri başarıyla silindi."));
    }
}
