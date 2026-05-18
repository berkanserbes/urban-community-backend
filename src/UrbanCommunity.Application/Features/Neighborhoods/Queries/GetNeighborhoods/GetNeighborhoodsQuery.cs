using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoods;

public sealed record GetNeighborhoodsQuery(
    string? City = null,
    string? District = null) : IRequest<Result<IReadOnlyList<GetNeighborhoodsQueryResponse>>>;
