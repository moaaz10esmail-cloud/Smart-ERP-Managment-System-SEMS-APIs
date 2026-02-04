using SEMS.Core.Common;
using SEMS.Core.ValueObjects;
using SEMS.Core.Enums;

namespace SEMS.Core.Finance;

public sealed class Transaction : BaseEntity
{
    public Guid BankAccountId { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid? ExpenseId { get; set; }
    public Guid? BudgetId { get; set; }
    public Money Amount { get; set; } = new Money(0, "USD");
    public PaymentDirection Direction { get; set; } = PaymentDirection.In;
    public DateTime OccurredOn { get; set; }
    public string Description { get; set; } = string.Empty;
}
