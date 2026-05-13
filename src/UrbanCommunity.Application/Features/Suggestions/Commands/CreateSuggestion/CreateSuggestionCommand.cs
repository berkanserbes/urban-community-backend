using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.CreateSuggestion;

public sealed record CreateSuggestionCommand(
    string Title,
    string Content,
    Guid UserId = default) : IRequest<Result<CreateSuggestionCommandResponse>>;
