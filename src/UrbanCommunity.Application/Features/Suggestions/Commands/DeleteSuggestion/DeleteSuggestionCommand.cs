using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.DeleteSuggestion;

public sealed record DeleteSuggestionCommand(Guid Id, Guid UserId, bool IsAdmin = false)
    : IRequest<Result<DeleteSuggestionCommandResponse>>;
