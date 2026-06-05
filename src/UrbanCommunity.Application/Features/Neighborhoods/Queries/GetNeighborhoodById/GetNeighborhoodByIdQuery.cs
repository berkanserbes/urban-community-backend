using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodById;

public sealed record GetNeighborhoodByIdQuery(Guid NeighborhoodId)
    : IRequest<Result<GetNeighborhoodByIdQueryResponse>>;
