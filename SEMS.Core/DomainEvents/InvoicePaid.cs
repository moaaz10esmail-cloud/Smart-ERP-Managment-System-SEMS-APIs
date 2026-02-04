using SEMS.Core.Common;

namespace SEMS.Core.DomainEvents;

public sealed class InvoicePaid : IDomainEvent
{
    public Guid InvoiceId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public InvoicePaid(Guid invoiceId) => InvoiceId = invoiceId;
}

