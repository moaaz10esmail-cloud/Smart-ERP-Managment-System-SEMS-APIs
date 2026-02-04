using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SEMS.Background.Services;

public class InvoiceProcessingService : BackgroundService
{
    private readonly ILogger<InvoiceProcessingService> _logger;
    public InvoiceProcessingService(ILogger<InvoiceProcessingService> logger) => _logger = logger;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Processing invoices");
            await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
        }
    }
}

