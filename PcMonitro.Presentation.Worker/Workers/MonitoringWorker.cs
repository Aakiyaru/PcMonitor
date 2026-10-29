using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PcMonitor.Application.UseCases;
using PcMonitor.Presentation.Worker.Options;

namespace PcMonitor.Presentation.Worker.Workers;

public sealed class MonitoringWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<WorkerOptions> _options;
    private readonly ILogger<MonitoringWorker> _logger;

    public MonitoringWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<WorkerOptions> options,
        ILogger<MonitoringWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.Value.SamplingIntervalSeconds);
        _logger.LogInformation("Monitoring worker started, interval {Interval}", interval);

        using var timer = new PeriodicTimer(interval);

        // Первый тик — сразу, не ждём interval.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;   // штатная остановка
            }
            catch (Exception ex)
            {
                // Один упавший тик не должен останавливать сбор.
                _logger.LogError(ex, "Failed to collect snapshot");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Monitoring worker stopped");
    }

    private async Task CollectOnceAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<CollectSnapshotUseCase>();
        await useCase.ExecuteAsync(ct);
    }
}