using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.CreateSuggestion;

public sealed class CreateSuggestionCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateSuggestionCommand, Result<CreateSuggestionCommandResponse>>
{
    public async Task<Result<CreateSuggestionCommandResponse>> Handle(CreateSuggestionCommand request, CancellationToken cancellationToken)
    {
        var suggestion = new Suggestion
        {
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            UserId = request.UserId,
            Status = SuggestionStatus.Active,
            VoteScore = 0
        };

        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync(cancellationToken);

        return Result<CreateSuggestionCommandResponse>.Success(
            new CreateSuggestionCommandResponse(
                suggestion.Id,
                suggestion.Title,
                suggestion.Status.ToString(),
                suggestion.CreatedAt));
    }
}
