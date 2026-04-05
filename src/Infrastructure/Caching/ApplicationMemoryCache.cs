using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using OrderManagement.Application.Common.Interfaces;

public class ApplicationMemoryCache : IApplicationCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public ApplicationMemoryCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<TResponse?> GetAsync<TResponse>(string key, CancellationToken cancellationToken)
    {
        _memoryCache.TryGetValue(key, out TResponse? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<TResponse>(string key, TResponse value, TimeSpan expiration, CancellationToken cancellationToken)
    {
        _memoryCache.Set(key, value, expiration);
        _keys[key] = 0;
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken)
    {
        var keys = _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList();

        foreach (var key in keys)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}