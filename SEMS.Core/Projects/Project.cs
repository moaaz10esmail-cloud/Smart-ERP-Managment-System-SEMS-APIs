using SEMS.Core.Common;
using SEMS.Core.Enums;

namespace SEMS.Core.Projects;

public sealed class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

