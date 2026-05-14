using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Votes.Commands.RemoveSuggestionVote;

public sealed record RemoveSuggestionVoteCommand(Guid SuggestionId, Guid UserId)
    : IRequest<Result<RemoveSuggestionVoteCommandResponse>>;
