using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dyconits.Services;

internal class DyconitHostedService : IHostedService
{
    private readonly ILogger<DyconitHostedService> _logger;

    public DyconitHostedService(ILogger<DyconitHostedService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DyconitHostedService started.");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DyconitHostedService stopped.");

        return Task.CompletedTask;
    }
}