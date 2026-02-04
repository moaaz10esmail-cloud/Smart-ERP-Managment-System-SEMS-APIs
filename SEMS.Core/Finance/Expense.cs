using SEMS.Core.Common;
using SEMS.Core.ValueObjects;

namespace SEMS.Core.Finance;

public sealed class Expense : BaseEntity
{
    public string Category { get; set; } = string.Empty;
    public Money Amount { get; set; } = new Money(0, "USD");
    public DateTime IncurredOn { get; set; }
}

