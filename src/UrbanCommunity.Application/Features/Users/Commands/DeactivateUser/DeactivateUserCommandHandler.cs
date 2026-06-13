using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler(
    IApplicationDbContext context,
    IRefreshTokenRevocationService refreshTokenRevocationService)
    : IRequestHandler<DeactivateUserCommand, Result<DeactivateUserCommandResponse>>
{
    public async Task<Result<DeactivateUserCommandResponse>> Handle(
        DeactivateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(user => user.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        user.Deactivate();

        await refreshTokenRevocationService.MarkAllActiveAsRevokedAsync(
            user.Id,
            "User deactivated",
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result<DeactivateUserCommandResponse>.Success(
            new DeactivateUserCommandResponse(
                user.Id,
                user.IsActive,
                "Kullanıcı hesabı başarıyla devre dışı bırakıldı."));
    }
}
