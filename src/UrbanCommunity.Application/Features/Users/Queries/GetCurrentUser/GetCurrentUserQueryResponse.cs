using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQueryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    UserRole Role,
    bool IsActive,
    Guid? ProvinceId,
    string? ProvinceName,
    Guid? DistrictId,
    string? DistrictName,
    Guid? NeighborhoodId,
    string? NeighborhoodName,
    Guid? DepartmentId,
    string? DepartmentName,
    string? Bio,
    string? ProfileImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
