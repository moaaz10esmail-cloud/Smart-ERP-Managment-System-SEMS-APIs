using SEMS.Core.Common;
using SEMS.Core.Enums;
using SEMS.Core.ValueObjects;

namespace SEMS.Core.Finance;

public sealed class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public Money Amount { get; set; } = new Money(0, "USD");
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentDirection Direction { get; set; } = PaymentDirection.In;
    public DateTime PaidOn { get; set; }
}
