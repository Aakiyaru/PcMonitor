using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PcMonitor.Domain.Events;
using PcMonitor.Infrastructure.Events;

namespace PcMonitor.Presentation.Worker.Logging;

/// <summary>
/// Подписывается на все доменные события и пишет их в лог.
/// Здесь же — место для будущих подписчиков: уведомления в Telegram,
/// запись в audit-trail, отправка в SIEM.
/// </summary>
public sealed class DomainEventLoggingSubscriber : IHostedService
{
    private readonly InMemoryEventPublisher _publisher;
    private readonly ILogger<DomainEventLoggingSubscriber> _logger;

    public DomainEventLoggingSubscriber(
        InMemoryEventPublisher publisher,
        ILogger<DomainEventLoggingSubscriber> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _publisher.Subscribe<SnapshotCollected>(e =>
        {
            _logger.LogDebug(
                "Snapshot {Id} collected at {At}: CPU {Cpu:F1}%, MEM {Mem:F1}%",
                e.SnapshotId, e.CollectedAt,
                e.Cpu.Percent,
                e.Memory.UtilizationPercent);
            return Task.CompletedTask;
        });

        _publisher.Subscribe<DailyReportGenerated>(e =>
        {
            _logger.LogInformation(
                "Report {Id} for {Date} generated with {Count} anomalies",
                e.ReportId, e.Date, e.AnomalyCount);
            return Task.CompletedTask;
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}