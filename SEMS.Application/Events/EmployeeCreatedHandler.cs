using SEMS.Core.Common;
using SEMS.Core.DomainEvents;
using SEMS.Application.Abstractions;

namespace SEMS.Application.Events;

public sealed class EmployeeCreatedHandler : IDomainEventHandler<EmployeeCreated>
{
    private readonly IEmailService _email;
    public EmployeeCreatedHandler(IEmailService email) => _email = email;
    public Task HandleAsync(EmployeeCreated domainEvent, CancellationToken cancellationToken = default)
    {
        return _email.SendAsync("hr@example.com", "New employee created", $"Employee {domainEvent.EmployeeId}", cancellationToken);
    }
}
