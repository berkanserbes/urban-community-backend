using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid Id) : IRequest<Result<DeactivateUserCommandResponse>>;
