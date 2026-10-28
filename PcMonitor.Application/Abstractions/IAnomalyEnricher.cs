using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;

namespace PcMonitor.Application.Abstractions
{
    /// <summary>
    /// Дополняет уже найденные аномалии контекстом
    /// (например, корреляция с системными событиями).
    /// </summary>
    public interface IAnomalyEnricher
    {
        Task<IReadOnlyList<Anomaly>> EnrichAsync(
            IReadOnlyList<Anomaly> anomalies,
            AnalysisContext context,
            CancellationToken ct = default);
    }
}
