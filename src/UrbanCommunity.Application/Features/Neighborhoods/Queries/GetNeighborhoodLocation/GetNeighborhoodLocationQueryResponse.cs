namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodLocation;

public sealed record GetNeighborhoodLocationQueryResponse(
    Guid NeighborhoodId,
    string NeighborhoodName,
    Guid? DistrictId,
    string? DistrictName,
    Guid? ProvinceId,
    string? ProvinceName);
