namespace OrderManagement.Application.Common.Interfaces;

public interface IApplicationCache
{
    Task<TResponse?> GetAsync<TResponse>(string key, CancellationToken cancellationToken);
    Task SetAsync<TResponse>(string key, TResponse value, TimeSpan expiration, CancellationToken cancellationToken);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken);
}