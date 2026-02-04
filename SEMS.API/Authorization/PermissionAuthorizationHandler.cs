using Microsoft.AspNetCore.Authorization;
using SEMS.Application.Abstractions;
using SEMS.Core.Enums;
using System.Security.Claims;

namespace SEMS.API.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            return Task.CompletedTask;
        }

        // Get roles from claims
        // Note: ClaimTypes.Role is the standard claim type for roles
        var userRoles = context.User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        foreach (var roleStr in userRoles)
        {
            if (Enum.TryParse<UserRole>(roleStr, out var role))
            {
                var permissions = _permissionService.GetPermissions(role);
                if (permissions.Contains(requirement.Permission))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
        }

        return Task.CompletedTask;
    }
}
