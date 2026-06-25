using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.AdminUpdateUser;

public sealed record AdminUpdateUserCommandResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    UserRole Role,
    bool IsActive,
    Guid? NeighborhoodId,
    Guid? OrganizationDepartmentId,
    string? Bio,
    string? ProfileImageUrl);
