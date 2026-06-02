using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByProvince;

public sealed record GetUsersByProvinceQueryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    UserRole Role,
    string? Bio,
    string? ProfileImageUrl,
    Guid? NeighborhoodId,
    string? NeighborhoodName);
