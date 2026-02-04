using SEMS.Core.Common;

namespace SEMS.Core.Projects;

public sealed class Milestone : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}

