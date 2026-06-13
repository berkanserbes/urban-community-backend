using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.ChangeSuggestionStatus;

public sealed class ChangeSuggestionStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ChangeSuggestionStatusCommand, Result<ChangeSuggestionStatusCommandResponse>>
{
    public async Task<Result<ChangeSuggestionStatusCommandResponse>> Handle(
        ChangeSuggestionStatusCommand request,
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
                "Öneri durumunu değiştirmek için admin veya official yetkisi gereklidir.");
        }

        var suggestion = await context.Suggestions
            .FirstOrDefaultAsync(
                item => item.Id == request.SuggestionId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(Suggestion), request.SuggestionId);

        if (!suggestion.CanTransitionTo(request.NewStatus))
        {
            var allowed = string.Join(", ", suggestion.GetAllowedNextStatuses());
            return Result<ChangeSuggestionStatusCommandResponse>.Failure(
                [$"Geçersiz durum geçişi: {suggestion.Status} -> {request.NewStatus}. İzin verilen durumlar: {allowed}."]);
        }

        suggestion.ChangeStatus(request.NewStatus);
        await context.SaveChangesAsync(cancellationToken);

        return Result<ChangeSuggestionStatusCommandResponse>.Success(
            new ChangeSuggestionStatusCommandResponse(
                suggestion.Id,
                suggestion.Title,
                suggestion.Status,
                $"Öneri durumu '{request.NewStatus}' olarak güncellendi."));
    }
}
