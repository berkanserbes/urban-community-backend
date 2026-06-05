namespace UrbanCommunity.Application.Features.Districts.Queries.GetDistrictById;

public sealed record GetDistrictByIdQueryResponse(
    Guid Id,
    string Name,
    Guid ProvinceId,
    string ProvinceName);
