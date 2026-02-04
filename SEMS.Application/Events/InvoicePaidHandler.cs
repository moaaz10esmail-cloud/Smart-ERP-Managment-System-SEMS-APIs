using SEMS.Core.Common;
using SEMS.Core.DomainEvents;
using SEMS.Application.Abstractions;

namespace SEMS.Application.Events;

public sealed class InvoicePaidHandler : IDomainEventHandler<InvoicePaid>
{
    private readonly IMessageBus _bus;
    public InvoicePaidHandler(IMessageBus bus) => _bus = bus;
    public Task HandleAsync(InvoicePaid domainEvent, CancellationToken cancellationToken = default)
    {
        var payload = System.Text.Encoding.UTF8.GetBytes($"InvoicePaid:{domainEvent.InvoiceId}");
        return _bus.PublishAsync("finance.invoice.paid", payload, cancellationToken);
    }
}
