namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodById;

public sealed record GetNeighborhoodByIdQueryResponse(
    Guid Id,
    string Name,
    Guid? DistrictId,
    string? DistrictName,
    Guid? ProvinceId,
    string? ProvinceName);
