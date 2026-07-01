using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Organizations.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommandResponse(
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
