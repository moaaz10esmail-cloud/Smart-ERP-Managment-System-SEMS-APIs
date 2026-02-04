using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SEMS.Background.Services;

public class ScheduledReportsService : BackgroundService
{
    private readonly ILogger<ScheduledReportsService> _logger;
    public ScheduledReportsService(ILogger<ScheduledReportsService> logger) => _logger = logger;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Generating scheduled reports");
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}

