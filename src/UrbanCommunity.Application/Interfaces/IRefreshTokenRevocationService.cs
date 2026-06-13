namespace UrbanCommunity.Application.Interfaces;

public interface IRefreshTokenRevocationService
{
    Task<int> MarkAllActiveAsRevokedAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken);
}
