using SEMS.Core.Common;

namespace SEMS.Core.Projects;

public sealed class ProjectResource : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
}

