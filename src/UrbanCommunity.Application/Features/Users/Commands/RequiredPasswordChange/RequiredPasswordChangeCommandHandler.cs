using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.RequiredPasswordChange;

public sealed class RequiredPasswordChangeCommandHandler(
    UserManager<User> userManager,
    IRefreshTokenRevocationService refreshTokenRevocationService)
    : IRequestHandler<RequiredPasswordChangeCommand, Result<RequiredPasswordChangeCommandResponse>>
{
    public async Task<Result<RequiredPasswordChangeCommandResponse>> Handle(
        RequiredPasswordChangeCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
            ?? throw new NotFoundException(nameof(User), request.UserId);

        if (!user.MustChangePassword)
            return Result<RequiredPasswordChangeCommandResponse>.Failure(
                ["Bu hesap için zorunlu şifre değişikliği bulunmuyor."]);

        var result = await userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);
        if (!result.Succeeded)
        {
            return Result<RequiredPasswordChangeCommandResponse>.Failure(
                result.Errors.Select(error => error.Description));
        }

        user.CompleteRequiredPasswordChange();
        await userManager.UpdateSecurityStampAsync(user);
        await refreshTokenRevocationService.MarkAllActiveAsRevokedAsync(
            user.Id,
            "Required password change completed",
            cancellationToken);
        await userManager.UpdateAsync(user);

        return Result<RequiredPasswordChangeCommandResponse>.Success(
            new RequiredPasswordChangeCommandResponse(
                user.Id,
                "Şifreniz güncellendi. Yeni şifrenizle tekrar giriş yapın."));
    }
}
