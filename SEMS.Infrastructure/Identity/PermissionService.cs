using SEMS.Application.Abstractions;
using SEMS.Core.Enums;
using SEMS.Core.Identity;

namespace SEMS.Infrastructure.Identity;

public class PermissionService : IPermissionService
{
    private readonly Dictionary<UserRole, HashSet<string>> _rolePermissions;

    public PermissionService()
    {
        _rolePermissions = new Dictionary<UserRole, HashSet<string>>
        {
            [UserRole.Admin] = new() 
            { 
                Permissions.Employees.View, Permissions.Employees.Create, Permissions.Employees.Edit, Permissions.Employees.Delete,
                Permissions.Invoices.View, Permissions.Invoices.Create, Permissions.Invoices.Pay, Permissions.Invoices.Cancel,
                Permissions.Users.View, Permissions.Users.ManageRoles,
                Permissions.Tenants.View, Permissions.Tenants.Create,
                Permissions.Attendance.View, Permissions.Attendance.Create, Permissions.Attendance.Approve
            },
            [UserRole.HR] = new()
            {
                Permissions.Employees.View, Permissions.Employees.Create, Permissions.Employees.Edit,
                Permissions.Attendance.View, Permissions.Attendance.Create, Permissions.Attendance.Approve
            },
            [UserRole.Finance] = new()
            {
                Permissions.Invoices.View, Permissions.Invoices.Pay, Permissions.Invoices.Create
            },
            [UserRole.Manager] = new()
            {
                Permissions.Employees.View, Permissions.Invoices.View,
                Permissions.Attendance.View, Permissions.Attendance.Approve
            },
            [UserRole.Employee] = new()
            {
                Permissions.Attendance.Create // Can mark own attendance
            }
        };
    }

    public HashSet<string> GetPermissions(UserRole role)
    {
        return _rolePermissions.TryGetValue(role, out var permissions) ? permissions : new HashSet<string>();
    }
}
