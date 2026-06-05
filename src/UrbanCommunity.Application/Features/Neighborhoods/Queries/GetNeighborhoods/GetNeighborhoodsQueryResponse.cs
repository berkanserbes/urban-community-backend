namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoods;

public sealed record GetNeighborhoodsQueryResponse(
    Guid Id,
    string Name,
    Guid? DistrictId,
    string? DistrictName,
    Guid? ProvinceId,
    string? ProvinceName);
