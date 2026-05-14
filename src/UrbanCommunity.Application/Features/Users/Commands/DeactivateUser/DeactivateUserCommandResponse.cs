namespace UrbanCommunity.Application.Features.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommandResponse(
    Guid Id,
    bool IsActive,
    string Message);
