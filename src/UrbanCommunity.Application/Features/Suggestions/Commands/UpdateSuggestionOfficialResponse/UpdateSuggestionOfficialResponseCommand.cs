using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestionOfficialResponse;

public sealed record UpdateSuggestionOfficialResponseCommand(
    Guid SuggestionId,
    Guid RequestingUserId,
    string? OfficialResponse) : IRequest<Result<UpdateSuggestionOfficialResponseCommandResponse>>;
