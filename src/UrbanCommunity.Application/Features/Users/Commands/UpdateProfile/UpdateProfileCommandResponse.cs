namespace UrbanCommunity.Application.Features.Users.Commands.UpdateProfile;

public sealed record UpdateProfileCommandResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Bio,
    string? PhoneNumber,
    Guid? NeighborhoodId,
    string? ProfileImageUrl,
    DateTime UpdatedAt);
