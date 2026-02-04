using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SEMS.Background.Services;

public class NotificationService : BackgroundService
{
    private readonly ILogger<NotificationService> _logger;
    public NotificationService(ILogger<NotificationService> logger) => _logger = logger;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Sending notifications");
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}

