using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(UserManager<User> userManager)
    : IRequestHandler<UpdateProfileCommand, Result<UpdateProfileCommandResponse>>
{
    public async Task<Result<UpdateProfileCommandResponse>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        user.UpdateProfile(request.FirstName, request.LastName, request.Bio);
        user.AssignNeighborhood(request.NeighborhoodId);

        if (request.PhoneNumber != user.PhoneNumber)
        {
            var setPhoneResult = await userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                return Result<UpdateProfileCommandResponse>.Failure(
                    setPhoneResult.Errors.Select(e => e.Description));
            }
        }

        var identityResult = await userManager.UpdateAsync(user);

        if (!identityResult.Succeeded)
        {
            return Result<UpdateProfileCommandResponse>.Failure(
                identityResult.Errors.Select(e => e.Description));
        }

        return Result<UpdateProfileCommandResponse>.Success(new UpdateProfileCommandResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Bio,
            user.PhoneNumber,
            user.NeighborhoodId,
            user.ProfileImageUrl,
            user.UpdatedAt ?? DateTime.UtcNow));
    }
}
