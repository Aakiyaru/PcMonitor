using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Ports;

namespace PcMonitor.Application.Services
{
    public sealed class DailyReportBuilder : IReportBuilder
    {
        private readonly IClock _clock;

        public DailyReportBuilder(IClock clock) => _clock = clock;

        public Task<DailyReport> BuildAsync(
            DateOnly date,
            IReadOnlyList<ResourceSnapshot> snapshots,
            IReadOnlyList<Anomaly> anomalies,
            IReadOnlyList<TopConsumer> topConsumers,
            CancellationToken ct = default)
        {
            // Агрегат сам валидирует инварианты и сортирует коллекции.
            var report = new DailyReport(
                date: date,
                generatedAt: _clock.UtcNow,
                snapshotsAnalyzed: snapshots.Count,
                anomalies: anomalies,
                topConsumers: topConsumers);

            return Task.FromResult(report);
        }
    }
}
