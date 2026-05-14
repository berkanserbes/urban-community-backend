using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? Bio,
    Guid? NeighborhoodId,
    string? PhoneNumber) : IRequest<Result<UpdateProfileCommandResponse>>;
