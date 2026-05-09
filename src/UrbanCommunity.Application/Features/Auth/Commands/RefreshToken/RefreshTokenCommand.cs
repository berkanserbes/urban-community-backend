using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<RefreshTokenCommandResponse>>;
