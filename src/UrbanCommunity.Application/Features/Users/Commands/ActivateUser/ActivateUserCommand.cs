using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Commands.ActivateUser;

public sealed record ActivateUserCommand(Guid Id) : IRequest<Result<ActivateUserCommandResponse>>;
