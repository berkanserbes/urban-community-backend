using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    ITokenService tokenService,
    IApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenCommandResponse>>
{
    public async Task<Result<RefreshTokenCommandResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var clientIp = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        ClaimsPrincipal principal;

        try
        {
            principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        }
        catch
        {
            throw new InvalidTokenException("Geçersiz access token.");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new InvalidTokenException("Token içeriği geçersiz.");
        }

        var storedToken = await context.UserRefreshTokens
            .FirstOrDefaultAsync(t => t.Token == tokenService.HashToken(request.RefreshToken) && t.UserId == userId, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            throw new InvalidTokenException("Refresh token geçersiz veya süresi dolmuş.");
        }

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new InvalidTokenException("Token sahibi kullanıcı bulunamadı.");

        if (!user.IsActive)
        {
            throw new AccountDeactivatedException();
        }

        var newAccessToken = tokenService.GenerateToken(user);
        var newAccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
            int.TryParse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRY_MINUTES"), out var minutes)
                ? minutes : 10);
        var newRefreshTokenValue = tokenService.GenerateRefreshToken();

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = clientIp;
        storedToken.RevokedReason = "Token replaced by refresh";
        storedToken.ReplacedByToken = tokenService.HashToken(newRefreshTokenValue);

        var newRefreshToken = new UserRefreshToken
        {
            UserId = user.Id,
            Token = tokenService.HashToken(newRefreshTokenValue),
            ExpiresAt = DateTime.UtcNow.AddDays(
                int.TryParse(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRY_DAYS"), out var days)
                    ? days : 14),
            CreatedByIp = clientIp
        };

        context.UserRefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result<RefreshTokenCommandResponse>.Success(new RefreshTokenCommandResponse(
            newAccessToken,
            newAccessTokenExpiresAt,
            newRefreshTokenValue,
            newRefreshToken.ExpiresAt,
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Role.ToString()));
    }
}
