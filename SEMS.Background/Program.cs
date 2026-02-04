using Microsoft.Extensions.Hosting;
using Serilog;
using SEMS.Background.Services;

Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, log) => log.ReadFrom.Configuration(ctx.Configuration))
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<NotificationService>();
        services.AddHostedService<ScheduledReportsService>();
        services.AddHostedService<InvoiceProcessingService>();
    })
    .Build()
    .Run();
