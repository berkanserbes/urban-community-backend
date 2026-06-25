using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQueryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    bool IsActive,
    Guid? NeighborhoodId,
    string? NeighborhoodName,
    Guid? OrganizationDepartmentId,
    Guid? OrganizationId,
    string? OrganizationName,
    Guid? DepartmentId,
    string? DepartmentName,
    OfficialJurisdictionLevel? OfficialJurisdictionLevel,
    Guid? OfficialJurisdictionId,
    string? OfficialJurisdictionName,
    string? Bio,
    string? ProfileImageUrl,
    bool MustChangePassword,
    DateTime CreatedAt);
