using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IRequest<Result<IReadOnlyList<GetUsersQueryResponse>>>;
