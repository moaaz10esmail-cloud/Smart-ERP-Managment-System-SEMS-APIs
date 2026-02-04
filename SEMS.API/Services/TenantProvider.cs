using Microsoft.AspNetCore.Http;
using SEMS.Application.Abstractions;

namespace SEMS.API.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Priority 1: Header
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
            {
                if (Guid.TryParse(tenantIdHeader, out var tenantId))
                {
                    return tenantId;
                }
            }

            // Priority 2: Claim (JWT)
            var tenantClaim = context.User?.FindFirst("tenantid")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var claimTenantId))
            {
                return claimTenantId;
            }

            return null;
        }
    }
}