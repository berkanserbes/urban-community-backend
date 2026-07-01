using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Organizations.Queries.GetOrganizations;

public sealed record GetOrganizationsQueryResponse(
    Guid Id,
    string Name,
    OrganizationJurisdictionLevel JurisdictionLevel,
    Guid ProvinceId,
    string ProvinceName,
    Guid? DistrictId,
    string? DistrictName,
    Guid? NeighborhoodId,
    string? NeighborhoodName,
    bool IsActive,
    int DepartmentCount,
    int OfficialCount);
