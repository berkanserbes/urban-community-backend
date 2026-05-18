using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Auth.Commands.LogoutUser;

public sealed class LogoutUserCommandHandler(
    IApplicationDbContext context,
    ITokenService tokenService,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<LogoutUserCommand, Result<LogoutUserCommandResponse>>
{
    public async Task<Result<LogoutUserCommandResponse>> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new InvalidTokenException("Kimlik doğrulama bilgisi geçersiz.");
        }

        var clientIp = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var storedToken = await context.UserRefreshTokens
            .FirstOrDefaultAsync(t => t.Token == tokenService.HashToken(request.RefreshToken) && t.UserId == userId, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            throw new InvalidTokenException("Refresh token geçersiz veya süresi dolmuş.");
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = clientIp;
        storedToken.RevokedReason = "User logout";

        await context.SaveChangesAsync(cancellationToken);

        return Result<LogoutUserCommandResponse>.Success(new LogoutUserCommandResponse("Başarıyla çıkış yapıldı."));
    }
}
