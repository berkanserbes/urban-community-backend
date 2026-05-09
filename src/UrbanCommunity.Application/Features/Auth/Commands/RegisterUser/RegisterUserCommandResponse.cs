namespace UrbanCommunity.Application.Features.Auth.Commands.RegisterUser;

public sealed record RegisterUserCommandResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName);
