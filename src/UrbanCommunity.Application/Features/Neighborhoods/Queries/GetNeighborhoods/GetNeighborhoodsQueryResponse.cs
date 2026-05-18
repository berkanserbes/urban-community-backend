namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoods;

public sealed record GetNeighborhoodsQueryResponse(
    Guid Id,
    string Name,
    string District,
    string City);
