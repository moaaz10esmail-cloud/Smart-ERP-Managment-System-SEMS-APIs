using System;

namespace SEMS.Core.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

