using MediatR;
using SEMS.Application.Abstractions;
using System.Text.Json;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;
using System.Linq;

namespace SEMS.Application.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cache;
    private readonly ITenantProvider _tenantProvider;
    private readonly IUserContext _userContext;

    public CachingBehavior(ICacheService cache, ITenantProvider tenantProvider, IUserContext userContext)
    {
        _cache = cache;
        _tenantProvider = tenantProvider;
        _userContext = userContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 1. Handle Invalidation
        var invalidationAttrs = request.GetType().GetCustomAttributes<CacheInvalidationAttribute>();
        if (invalidationAttrs.Any())
        {
            var response = await next();
            foreach (var attr in invalidationAttrs)
            {
                var tenantId = _tenantProvider.TenantId?.ToString() ?? "global";
                var prefix = $"{tenantId}:{attr.KeyPrefix}";
                await _cache.RemoveByPrefixAsync(prefix);
            }
            return response;
        }

        // 2. Handle Caching
        var cacheAttr = request.GetType().GetCustomAttribute<CachedAttribute>();
        if (cacheAttr != null)
        {
            var tenantId = _tenantProvider.TenantId?.ToString() ?? "global";
            var key = GenerateCacheKey(request, cacheAttr.KeyPrefix, tenantId, _userContext.Roles);
            
            var cachedResponse = await _cache.GetAsync<TResponse>(key);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await next();
            await _cache.SetAsync(key, response, TimeSpan.FromMinutes(cacheAttr.ExpireInMinutes));
            return response;
        }

        return await next();
    }

    private string GenerateCacheKey(TRequest request, string prefix, string tenantId, IEnumerable<string> roles)
    {
        var sb = new StringBuilder();
        var rolesPart = string.Join(",", roles.OrderBy(r => r));
        sb.Append($"{tenantId}:{rolesPart}:{prefix}:");
        
        var props = request.GetType().GetProperties();
        foreach (var prop in props)
        {
            var value = prop.GetValue(request);
            sb.Append($"{prop.Name}:{value}|");
        }

        // Hash the parameters to keep key short
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        return $"{tenantId}:{rolesPart}:{prefix}:{hashString}";
    }
}
