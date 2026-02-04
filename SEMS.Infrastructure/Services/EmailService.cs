using SEMS.Application.Abstractions;

namespace SEMS.Infrastructure.Services;

public class EmailService : IEmailService
{
    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
