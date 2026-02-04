using SEMS.Core.Enums;

namespace SEMS.Application.Abstractions;

public interface IPermissionService
{
    HashSet<string> GetPermissions(UserRole role);
}
