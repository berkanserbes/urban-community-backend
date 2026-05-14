using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler(UserManager<User> userManager)
    : IRequestHandler<DeactivateUserCommand, Result<DeactivateUserCommandResponse>>
{
    public async Task<Result<DeactivateUserCommandResponse>> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.Id);
        }

        user.Deactivate();
        await userManager.UpdateAsync(user);

        return Result<DeactivateUserCommandResponse>.Success(
            new DeactivateUserCommandResponse(user.Id, user.IsActive, "Kullanıcı hesabı başarıyla devre dışı bırakıldı."));
    }
}
