namespace SEMS.Infrastructure.Services;

public interface ISmsService
{
    Task SendAsync(string phone, string message, CancellationToken cancellationToken = default);
}

public class SmsService : ISmsService
{
    public Task SendAsync(string phone, string message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

