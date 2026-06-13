using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateUserAccess;

public sealed record UpdateUserAccessCommandResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    Guid? DepartmentId,
    string? DepartmentName,
    OfficialJurisdictionLevel? OfficialJurisdictionLevel,
    Guid? OfficialJurisdictionId,
    string? OfficialJurisdictionName);
