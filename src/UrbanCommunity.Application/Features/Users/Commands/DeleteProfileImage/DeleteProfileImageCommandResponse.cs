namespace UrbanCommunity.Application.Features.Users.Commands.DeleteProfileImage;

public sealed record DeleteProfileImageCommandResponse(
    Guid UserId,
    string Message);
