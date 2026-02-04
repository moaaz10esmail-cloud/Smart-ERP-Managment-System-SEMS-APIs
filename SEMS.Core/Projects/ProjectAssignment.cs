using SEMS.Core.Common;

namespace SEMS.Core.Projects;

public sealed class ProjectAssignment : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string Role { get; set; } = string.Empty;
}

