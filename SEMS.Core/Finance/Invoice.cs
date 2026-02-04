using SEMS.Core.Common;
using SEMS.Core.Enums;
using SEMS.Core.ValueObjects;
using SEMS.Core.CRM;

namespace SEMS.Core.Finance;

public sealed class Invoice : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid BankAccountId { get; set; }
    public Money Total { get; set; } = new Money(0, "USD");
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime DueDate { get; set; }
}
