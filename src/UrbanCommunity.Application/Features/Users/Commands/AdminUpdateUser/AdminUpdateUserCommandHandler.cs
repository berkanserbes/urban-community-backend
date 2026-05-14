using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.AdminUpdateUser;

public sealed class AdminUpdateUserCommandHandler(UserManager<User> userManager)
    : IRequestHandler<AdminUpdateUserCommand, Result<AdminUpdateUserCommandResponse>>
{
    public async Task<Result<AdminUpdateUserCommandResponse>> Handle(
        AdminUpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.Id);
        }

        user.UpdateProfile(request.FirstName, request.LastName, request.Bio);
        user.AssignNeighborhood(request.NeighborhoodId);

        if (request.PhoneNumber != user.PhoneNumber)
        {
            var phoneResult = await userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
            if (!phoneResult.Succeeded)
            {
                return Result<AdminUpdateUserCommandResponse>.Failure(
                    phoneResult.Errors.Select(e => e.Description));
            }
        }

        var identityResult = await userManager.UpdateAsync(user);

        if (!identityResult.Succeeded)
        {
            return Result<AdminUpdateUserCommandResponse>.Failure(
                identityResult.Errors.Select(e => e.Description));
        }

        return Result<AdminUpdateUserCommandResponse>.Success(new AdminUpdateUserCommandResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.PhoneNumber,
            user.Role,
            user.IsActive,
            user.NeighborhoodId,
            user.DepartmentId,
            user.Bio,
            user.ProfileImageUrl));
    }
}
