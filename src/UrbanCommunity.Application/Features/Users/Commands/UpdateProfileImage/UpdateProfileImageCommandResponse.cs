namespace UrbanCommunity.Application.Features.Users.Commands.UpdateProfileImage;

public sealed record UpdateProfileImageCommandResponse(
    Guid UserId,
    string ProfileImageUrl);
