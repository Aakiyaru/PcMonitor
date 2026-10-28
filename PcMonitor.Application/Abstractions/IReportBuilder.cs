using PcMonitor.Domain.Entities;

namespace PcMonitor.Application.Abstractions
{
    public interface IReportBuilder
    {
        Task<DailyReport> BuildAsync(
            DateOnly date,
            IReadOnlyList<ResourceSnapshot> snapshots,
            IReadOnlyList<Anomaly> anomalies,
            IReadOnlyList<TopConsumer> topConsumers,
            CancellationToken ct = default);
    }
}
