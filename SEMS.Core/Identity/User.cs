using SEMS.Core.Common;
using SEMS.Core.Enums;

namespace SEMS.Core.Identity;

public sealed class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public List<UserRole> Roles { get; set; } = new();
}
