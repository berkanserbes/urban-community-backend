using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestion;

public sealed record UpdateSuggestionCommand(
    Guid Id,
    Guid UserId,
    string Title,
    string Content) : IRequest<Result<UpdateSuggestionCommandResponse>>;
