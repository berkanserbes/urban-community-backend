using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateUserAccess;

public sealed record UpdateUserAccessCommandResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    Guid? OrganizationDepartmentId,
    string? OrganizationName,
    string? DepartmentName,
    OfficialJurisdictionLevel? OfficialJurisdictionLevel,
    Guid? OfficialJurisdictionId,
    string? OfficialJurisdictionName);
