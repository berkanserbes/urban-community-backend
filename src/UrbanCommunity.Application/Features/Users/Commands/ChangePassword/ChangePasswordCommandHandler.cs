using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(UserManager<User> userManager)
    : IRequestHandler<ChangePasswordCommand, Result<ChangePasswordCommandResponse>>
{
    public async Task<Result<ChangePasswordCommandResponse>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        var identityResult = await userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!identityResult.Succeeded)
        {
            return Result<ChangePasswordCommandResponse>.Failure(identityResult.Errors.Select(e => e.Description));
        }

        await userManager.UpdateAsync(user);

        return Result<ChangePasswordCommandResponse>.Success(
            new ChangePasswordCommandResponse(user.Id, "Şifreniz başarıyla güncellendi."));
    }
}
