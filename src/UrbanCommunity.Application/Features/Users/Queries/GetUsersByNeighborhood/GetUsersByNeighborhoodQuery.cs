using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByNeighborhood;

public sealed record GetUsersByNeighborhoodQuery(Guid NeighborhoodId)
    : PaginationRequest, IRequest<Result<PagedResult<GetUsersByNeighborhoodQueryResponse>>>;
