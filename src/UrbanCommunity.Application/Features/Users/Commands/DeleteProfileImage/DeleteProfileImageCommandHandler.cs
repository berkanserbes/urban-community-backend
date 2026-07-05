using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.DeleteProfileImage;

public sealed class DeleteProfileImageCommandHandler(
    UserManager<User> userManager,
    IFileService fileService,
    ILogger<DeleteProfileImageCommandHandler> logger)
    : IRequestHandler<DeleteProfileImageCommand, Result<DeleteProfileImageCommandResponse>>
{
    public async Task<Result<DeleteProfileImageCommandResponse>> Handle(DeleteProfileImageCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        if (string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            return Result<DeleteProfileImageCommandResponse>.Failure(["Kullanıcının profil fotoğrafı bulunmamaktadır."]);
        }

        var previousImageUrl = user.ProfileImageUrl;

        user.SetProfileImage(null);

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result<DeleteProfileImageCommandResponse>.Failure(
                updateResult.Errors.Select(error => error.Description).ToArray());
        }

        await DeleteFileBestEffortAsync(previousImageUrl, cancellationToken);

        return Result<DeleteProfileImageCommandResponse>.Success(
            new DeleteProfileImageCommandResponse(user.Id, "Profil fotoğrafı başarıyla silindi."));
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
