namespace UrbanCommunity.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommandResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool MustChangePassword);
