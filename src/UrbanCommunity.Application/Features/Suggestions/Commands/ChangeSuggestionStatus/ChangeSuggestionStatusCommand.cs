using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.ChangeSuggestionStatus;

public sealed record ChangeSuggestionStatusCommand(
    Guid SuggestionId,
    SuggestionStatus NewStatus) : IRequest<Result<ChangeSuggestionStatusCommandResponse>>;
