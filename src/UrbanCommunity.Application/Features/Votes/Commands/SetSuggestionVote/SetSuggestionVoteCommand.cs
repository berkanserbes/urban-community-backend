using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Votes.Commands.SetSuggestionVote;

public sealed record SetSuggestionVoteCommand(Guid SuggestionId, Guid UserId, bool IsUpvote)
    : IRequest<Result<SetSuggestionVoteCommandResponse>>;
