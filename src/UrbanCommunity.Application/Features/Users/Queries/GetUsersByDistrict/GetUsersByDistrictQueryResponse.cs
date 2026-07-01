using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByDistrict;

public sealed record GetUsersByDistrictQueryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    UserRole Role,
    string? Bio,
    string? ProfileImageUrl,
    Guid? NeighborhoodId,
    string? NeighborhoodName,
    Guid? OrganizationDepartmentId,
    Guid? OrganizationId,
    string? OrganizationName,
    Guid? DepartmentId,
    string? DepartmentName,
    OfficialJurisdictionLevel? OfficialJurisdictionLevel,
    Guid? OfficialJurisdictionId,
    string? OfficialJurisdictionName);
