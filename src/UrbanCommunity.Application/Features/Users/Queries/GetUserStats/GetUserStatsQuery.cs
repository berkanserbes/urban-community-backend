using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUserStats;

public sealed record GetUserStatsQuery : IRequest<Result<GetUserStatsQueryResponse>>;
