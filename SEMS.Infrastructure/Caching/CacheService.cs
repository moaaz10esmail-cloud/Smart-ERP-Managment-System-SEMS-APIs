using StackExchange.Redis;
using System.Text.Json;
using System.Linq;
using SEMS.Application.Abstractions;

namespace SEMS.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _muxer;
    private readonly IDatabase _db;
    
    public RedisCacheService(string connectionString)
    {
        _muxer = ConnectionMultiplexer.Connect(connectionString);
        _db = _muxer.GetDatabase();
    }
    
    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        return _db.StringSetAsync(key, json, expiry: expiry);
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        var v = await _db.StringGetAsync(key);
        return v.HasValue ? JsonSerializer.Deserialize<T>(v.ToString()) : default;
    }

    public Task RemoveAsync(string key) => _db.KeyDeleteAsync(key);

    public async Task RemoveByPrefixAsync(string prefix)
    {
        // Note: In production, use SCAN instead of Keys. This is simplified.
        var server = _muxer.GetServer(_muxer.GetEndPoints().First());
        var keys = server.Keys(pattern: prefix + "*");
        foreach (var key in keys)
        {
            await _db.KeyDeleteAsync(key);
        }
    }
}

public class InMemoryCacheService : ICacheService
{
    private readonly Dictionary<string, (string val, DateTime? exp)> _cache = new();
    
    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        lock (_cache)
        {
            _cache[key] = (json, expiry.HasValue ? DateTime.UtcNow + expiry : null);
        }
        return Task.CompletedTask;
    }
    
    public Task<T?> GetAsync<T>(string key)
    {
        string? json = null;
        lock (_cache)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.exp.HasValue && entry.exp.Value < DateTime.UtcNow)
                {
                    _cache.Remove(key);
                }
                else
                {
                    json = entry.val;
                }
            }
        }
        
        return Task.FromResult(json != null ? JsonSerializer.Deserialize<T>(json) : default);
    }

    public Task RemoveAsync(string key)
    {
        lock (_cache)
        {
            _cache.Remove(key);
        }
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        lock (_cache)
        {
            var keysToRemove = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var k in keysToRemove) _cache.Remove(k);
        }
        return Task.CompletedTask;
    }
}
