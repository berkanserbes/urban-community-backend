using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Services;

public sealed class RefreshTokenRevocationService(IApplicationDbContext context)
    : IRefreshTokenRevocationService
{
    public async Task<int> MarkAllActiveAsRevokedAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var activeTokens = await context.UserRefreshTokens
            .Where(token =>
                token.UserId == userId &&
                !token.RevokedAt.HasValue &&
                token.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
            token.RevokedReason = reason;
        }

        return activeTokens.Count;
    }
}
