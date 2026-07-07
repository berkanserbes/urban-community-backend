using MediatR;
using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Behaviors;

public sealed class CacheInvalidationBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        if (request is not ICacheInvalidatingCommand invalidatingCommand)
        {
            return response;
        }

        if (response is not Result { IsSuccess: true })
        {
            return response;
        }

        foreach (var cacheKey in invalidatingCommand.CacheKeysToRemove)
        {
            try
            {
                await cacheService.RemoveAsync(cacheKey, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Cache key invalidation failed for key {CacheKey}.",
                    cacheKey);
            }
        }

        foreach (var cachePrefix in invalidatingCommand.CachePrefixesToRemove)
        {
            try
            {
                await cacheService.RemoveByPrefixAsync(cachePrefix, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Cache prefix invalidation failed for prefix {CachePrefix}.",
                    cachePrefix);
            }
        }

        return response;
    }
}
