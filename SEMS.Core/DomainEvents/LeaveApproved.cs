using SEMS.Core.Common;

namespace SEMS.Core.DomainEvents;

public sealed class LeaveApproved : IDomainEvent
{
    public Guid LeaveRequestId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public LeaveApproved(Guid leaveRequestId) => LeaveRequestId = leaveRequestId;
}

