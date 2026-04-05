using OrderManagement.Application.Common.Interfaces;

public class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationCache _cache;

    public CachingBehaviour(IApplicationCache cache)
    {
        _cache = cache;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken
    )
    {
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next();
        }

        var cachedResponse = await _cache.GetAsync<TResponse>(cacheableQuery.CacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        var response = await next();

        await _cache.SetAsync(cacheableQuery.CacheKey, response, cacheableQuery.Expiration, cancellationToken);

        return response;
    }
}