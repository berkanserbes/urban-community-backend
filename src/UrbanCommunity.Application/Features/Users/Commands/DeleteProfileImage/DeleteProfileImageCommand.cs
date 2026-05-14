using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Commands.DeleteProfileImage;

public sealed record DeleteProfileImageCommand(Guid UserId) : IRequest<Result<DeleteProfileImageCommandResponse>>;
