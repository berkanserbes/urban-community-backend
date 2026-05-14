using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.AssignRole;

public sealed record AssignRoleCommandResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    Guid? DepartmentId);
