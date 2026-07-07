using MediatR;
using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next(cancellationToken);
        }

        try
        {
            var cachedResponse = await cacheService.GetAsync<TResponse>(
                cacheableQuery.CacheKey,
                cancellationToken);

            if (cachedResponse is not null)
            {
                return cachedResponse;
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Cache read failed for key {CacheKey}. Continuing without cached value.",
                cacheableQuery.CacheKey);
        }

        var response = await next(cancellationToken);

        if (response is Result { IsSuccess: false })
        {
            return response;
        }

        try
        {
            await cacheService.SetAsync(
                cacheableQuery.CacheKey,
                response,
                cacheableQuery.CacheOptions,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Cache write failed for key {CacheKey}. Returning uncached response.",
                cacheableQuery.CacheKey);
        }

        return response;
    }
}
