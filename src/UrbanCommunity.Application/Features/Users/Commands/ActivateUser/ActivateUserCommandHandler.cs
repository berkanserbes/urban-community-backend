using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.ActivateUser;

public sealed class ActivateUserCommandHandler(UserManager<User> userManager)
    : IRequestHandler<ActivateUserCommand, Result<ActivateUserCommandResponse>>
{
    public async Task<Result<ActivateUserCommandResponse>> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.Id);
        }

        user.Activate();
        await userManager.UpdateAsync(user);

        return Result<ActivateUserCommandResponse>.Success(
            new ActivateUserCommandResponse(user.Id, user.IsActive, "Kullanıcı hesabı başarıyla aktifleştirildi."));
    }
}
