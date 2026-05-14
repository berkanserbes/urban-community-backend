using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Commands.AdminUpdateUser;

public sealed record AdminUpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? Bio,
    string? PhoneNumber,
    Guid? NeighborhoodId) : IRequest<Result<AdminUpdateUserCommandResponse>>;
