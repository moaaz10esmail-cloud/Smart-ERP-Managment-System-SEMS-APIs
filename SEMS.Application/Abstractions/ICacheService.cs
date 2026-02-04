namespace SEMS.Application.Abstractions;

public interface ICacheService
{
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
}
