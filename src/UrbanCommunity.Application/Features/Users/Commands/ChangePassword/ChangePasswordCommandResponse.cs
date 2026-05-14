namespace UrbanCommunity.Application.Features.Users.Commands.ChangePassword;

public sealed record ChangePasswordCommandResponse(
    Guid UserId,
    string Message);
