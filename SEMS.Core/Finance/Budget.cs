using SEMS.Core.Common;
using SEMS.Core.ValueObjects;

namespace SEMS.Core.Finance;

public sealed class Budget : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Money Amount { get; set; } = new Money(0, "USD");
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

