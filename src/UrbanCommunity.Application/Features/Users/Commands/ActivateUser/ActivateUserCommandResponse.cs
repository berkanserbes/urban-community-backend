namespace UrbanCommunity.Application.Features.Users.Commands.ActivateUser;

public sealed record ActivateUserCommandResponse(
    Guid Id,
    bool IsActive,
    string Message);
