using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateProfileImage;

public sealed class UpdateProfileImageCommandHandler(
    UserManager<User> userManager,
    IFileService fileService,
    ILogger<UpdateProfileImageCommandHandler> logger)
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

        var previousImageUrl = user.ProfileImageUrl;

        string imageUrl;

        try
        {
            imageUrl = await fileService.UploadFileAsync(request.Image, "profile-images", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Result<UpdateProfileImageCommandResponse>.Failure(["Fotoğraf yüklenemedi. Lütfen tekrar deneyin."]);
        }

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return Result<UpdateProfileImageCommandResponse>.Failure(["Fotoğraf yüklenemedi. Lütfen tekrar deneyin."]);
        }

        user.SetProfileImage(imageUrl);

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            await DeleteFileBestEffortAsync(imageUrl, cancellationToken);
            return Result<UpdateProfileImageCommandResponse>.Failure(
                updateResult.Errors.Select(error => error.Description).ToArray());
        }

        if (!string.IsNullOrWhiteSpace(previousImageUrl))
        {
            await DeleteFileBestEffortAsync(previousImageUrl, cancellationToken);
        }

        return Result<UpdateProfileImageCommandResponse>.Success(
            new UpdateProfileImageCommandResponse(user.Id, imageUrl));
    }

    private async Task DeleteFileBestEffortAsync(string fileUrl, CancellationToken cancellationToken)
    {
        try
        {
            await fileService.DeleteFileAsync(fileUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Profile image cleanup failed for {FileUrl}.", fileUrl);
        }
    }
}
