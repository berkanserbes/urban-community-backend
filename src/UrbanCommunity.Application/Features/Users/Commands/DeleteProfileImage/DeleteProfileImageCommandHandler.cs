using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.DeleteProfileImage;

public sealed class DeleteProfileImageCommandHandler(
    UserManager<User> userManager,
    IFileService fileService)
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

        await fileService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);

        user.SetProfileImage(null);

        await userManager.UpdateAsync(user);

        return Result<DeleteProfileImageCommandResponse>.Success(
            new DeleteProfileImageCommandResponse(user.Id, "Profil fotoğrafı başarıyla silindi."));
    }
}
