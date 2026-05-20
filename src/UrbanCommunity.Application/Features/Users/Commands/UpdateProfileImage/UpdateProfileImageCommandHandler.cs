using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateProfileImage;

public sealed class UpdateProfileImageCommandHandler(
    UserManager<User> userManager,
    IFileService fileService)
    : IRequestHandler<UpdateProfileImageCommand, Result<UpdateProfileImageCommandResponse>>
{
    public async Task<Result<UpdateProfileImageCommandResponse>> Handle(
        UpdateProfileImageCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            await fileService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);
        }

        var imageUrl = await fileService.UploadFileAsync(request.Image, "profile-images", cancellationToken);

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return Result<UpdateProfileImageCommandResponse>.Failure(["Fotoğraf yüklenemedi. Lütfen tekrar deneyin."]);
        }

        user.SetProfileImage(imageUrl);

        await userManager.UpdateAsync(user);

        return Result<UpdateProfileImageCommandResponse>.Success(
            new UpdateProfileImageCommandResponse(user.Id, imageUrl));
    }
}
