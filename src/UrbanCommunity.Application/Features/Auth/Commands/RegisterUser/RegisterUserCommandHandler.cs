using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    UserManager<User> userManager) : IRequestHandler<RegisterUserCommand, Result<RegisterUserCommandResponse>>
{
    public async Task<Result<RegisterUserCommandResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            UserName = request.Email.Trim().ToLowerInvariant(),
            NeighborhoodId = request.NeighborhoodId
        };

        var identityResult = await userManager.CreateAsync(user, request.Password);

        if (!identityResult.Succeeded)
        {
            return Result<RegisterUserCommandResponse>.Failure(
                identityResult.Errors.Select(e => e.Description));
        }

        return Result<RegisterUserCommandResponse>.Success(
            new RegisterUserCommandResponse(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName));
    }
}
