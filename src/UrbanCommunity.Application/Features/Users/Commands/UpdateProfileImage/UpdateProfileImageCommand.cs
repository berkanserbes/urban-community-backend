using MediatR;
using Microsoft.AspNetCore.Http;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateProfileImage;

public sealed record UpdateProfileImageCommand(
    Guid UserId,
    IFormFile Image) : IRequest<Result<UpdateProfileImageCommandResponse>>;
