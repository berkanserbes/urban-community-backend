using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodLocation;

public sealed record GetNeighborhoodLocationQuery(Guid NeighborhoodId)
    : IRequest<Result<GetNeighborhoodLocationQueryResponse>>;
