using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PcMonitor.Application.Abstractions;
using PcMonitor.Application.UseCases;
using PcMonitor.Domain.Ports;
using PcMonitor.Presentation.Worker.Options;

namespace PcMonitor.Presentation.Worker.Workers;

public sealed class DailyReportWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<WorkerOptions> _options;
    private readonly ILogger<DailyReportWorker> _logger;

    public DailyReportWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<WorkerOptions> options,
        ILogger<DailyReportWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.Value.ReportCheckIntervalSeconds);
        _logger.LogInformation("Daily report worker started, check interval {Interval}", interval);

        using var timer = new PeriodicTimer(interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnsureYesterdayReportAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ensure daily report");
            }

            try { await timer.WaitForNextTickAsync(stoppingToken); }
            catch (OperationCanceledException) { break; }
        }

        _logger.LogInformation("Daily report worker stopped");
    }

    private async Task EnsureYesterdayReportAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var reportRepo = scope.ServiceProvider.GetRequiredService<IDailyReportRepository>();

        // «Вчера» относительно текущего времени — но безопасно к перезапускам:
        // если отчёт за вчера уже есть, ничего не делаем.
        var yesterday = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime).AddDays(-1);

        var existing = await reportRepo.GetAsync(yesterday, ct);
        if (existing is not null)
        {
            _logger.LogDebug("Report for {Date} already exists, skipping", yesterday);
            return;
        }

        _logger.LogInformation("Generating report for {Date}", yesterday);

        var useCase = scope.ServiceProvider.GetRequiredService<GenerateDailyReportUseCase>();
        var report = await useCase.ExecuteAsync(yesterday, ct);

        _logger.LogInformation(
            "Report for {Date} generated: {Snapshots} snapshots, " +
            "{Critical} critical, {Warnings} warnings, {Anomalies} total anomalies",
            yesterday,
            report.SnapshotsAnalyzed,
            report.CriticalAnomalyCount,
            report.WarningAnomalyCount,
            report.Anomalies.Count);
    }
}