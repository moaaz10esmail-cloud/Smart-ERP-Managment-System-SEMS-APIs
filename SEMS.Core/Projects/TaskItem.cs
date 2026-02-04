using SEMS.Core.Common;
using ProjectTaskStatus = SEMS.Core.Enums.TaskStatus;

namespace SEMS.Core.Projects;

public sealed class TaskItem : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.Todo;
}
