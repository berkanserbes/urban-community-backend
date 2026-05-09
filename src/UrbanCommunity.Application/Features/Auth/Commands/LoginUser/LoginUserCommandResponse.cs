namespace UrbanCommunity.Application.Features.Auth.Commands.LoginUser;

public sealed record LoginUserCommandResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role);
