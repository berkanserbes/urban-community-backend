using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Auth.Commands.LogoutUser;

public sealed record LogoutUserCommand(string RefreshToken) : IRequest<Result<LogoutUserCommandResponse>>;
