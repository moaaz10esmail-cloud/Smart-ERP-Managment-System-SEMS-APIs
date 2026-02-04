using SEMS.Core.Common;

namespace SEMS.Core.DomainEvents;

public sealed class ProjectCreated : IDomainEvent
{
    public Guid ProjectId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public ProjectCreated(Guid projectId) => ProjectId = projectId;
}

