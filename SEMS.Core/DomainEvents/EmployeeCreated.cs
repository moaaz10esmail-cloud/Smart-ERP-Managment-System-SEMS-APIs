using SEMS.Core.Common;

namespace SEMS.Core.DomainEvents;

public sealed class EmployeeCreated : IDomainEvent
{
    public Guid EmployeeId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public EmployeeCreated(Guid employeeId) => EmployeeId = employeeId;
}

