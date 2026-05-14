using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQueryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    bool IsActive,
    Guid? NeighborhoodId,
    Guid? DepartmentId,
    string? Bio,
    string? ProfileImageUrl,
    DateTime CreatedAt);
