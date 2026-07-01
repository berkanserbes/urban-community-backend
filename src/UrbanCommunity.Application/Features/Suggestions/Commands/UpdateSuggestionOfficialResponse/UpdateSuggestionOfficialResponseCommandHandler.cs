using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestionOfficialResponse;

public sealed class UpdateSuggestionOfficialResponseCommandHandler(
    IApplicationDbContext context,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<UpdateSuggestionOfficialResponseCommand, Result<UpdateSuggestionOfficialResponseCommandResponse>>
{
    public async Task<Result<UpdateSuggestionOfficialResponseCommandResponse>> Handle(
        UpdateSuggestionOfficialResponseCommand request,
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
                "Resmi cevabi guncellemek icin admin veya official yetkisi gereklidir.");
        }

        var suggestion = await context.Suggestions
            .FirstOrDefaultAsync(
                item => item.Id == request.SuggestionId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(Suggestion), request.SuggestionId);

        var hadResponseBefore = suggestion.OfficialResponse is not null;
        suggestion.OfficialResponse = string.IsNullOrWhiteSpace(request.OfficialResponse)
            ? null
            : request.OfficialResponse.Trim();

        await context.SaveChangesAsync(cancellationToken);

        if (suggestion.OfficialResponse is not null)
        {
            await publishEndpoint.Publish(new SuggestionOfficialResponseUpdatedEvent
            {
                SuggestionId = suggestion.Id,
                SuggestionTitle = suggestion.Title,
                SuggestionOwnerId = suggestion.UserId,
                UpdatedAt = suggestion.UpdatedAt ?? DateTime.UtcNow
            }, cancellationToken);
        }

        return Result<UpdateSuggestionOfficialResponseCommandResponse>.Success(
            new UpdateSuggestionOfficialResponseCommandResponse(
                suggestion.Id,
                suggestion.OfficialResponse,
                suggestion.UpdatedAt,
                suggestion.OfficialResponse is null
                    ? "Resmi cevap temizlendi."
                    : "Resmi cevap guncellendi."));
    }
}
