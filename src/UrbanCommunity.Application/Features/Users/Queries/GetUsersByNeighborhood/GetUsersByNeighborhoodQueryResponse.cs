using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByNeighborhood;

public sealed record GetUsersByNeighborhoodQueryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    UserRole Role,
    string? Bio,
    string? ProfileImageUrl,
    Guid? NeighborhoodId,
    string? NeighborhoodName);
