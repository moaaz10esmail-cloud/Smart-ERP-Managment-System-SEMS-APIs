using SEMS.Core.Common;

namespace SEMS.Core.DomainEvents;

public sealed class TaskCompleted : IDomainEvent
{
    public Guid TaskId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public TaskCompleted(Guid taskId) => TaskId = taskId;
}

