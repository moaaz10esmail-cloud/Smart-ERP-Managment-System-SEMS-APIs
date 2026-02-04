using SEMS.Core.Common;
using SEMS.Core.ValueObjects;

namespace SEMS.Core.Finance;

public sealed class BankAccount : BaseEntity
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public Money Balance { get; set; } = new Money(0, "USD");
}
