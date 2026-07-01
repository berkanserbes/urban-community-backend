using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.CreateOfficial;

public sealed record CreateOfficialCommandResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    Guid OrganizationDepartmentId,
    string OrganizationName,
    string DepartmentName,
    OfficialJurisdictionLevel OfficialJurisdictionLevel,
    Guid OfficialJurisdictionId,
    string OfficialJurisdictionName,
    string TemporaryPassword,
    bool MustChangePassword);
