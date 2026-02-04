using SEMS.Core.Common;

namespace SEMS.Core.Auditing;

public class AuditLog : BaseEntity
{
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; }
}

