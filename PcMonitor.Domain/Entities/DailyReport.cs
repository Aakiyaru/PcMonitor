using PcMonitor.Domain.Common;
using PcMonitor.Domain.Enums;

namespace PcMonitor.Domain.Entities;

/// <summary>
/// Агрегат: ежедневный отчёт, собранный из снимков за сутки.
/// </summary>
public sealed class DailyReport
{
    private readonly List<Anomaly> _anomalies;
    private readonly List<TopConsumer> _topConsumers;
    private readonly IReadOnlyList<Anomaly> _anomaliesReadOnly;
    private readonly IReadOnlyList<TopConsumer> _topConsumersReadOnly;

    public Guid Id { get; }
    public DateOnly Date { get; }
    public DateTimeOffset GeneratedAt { get; }
    public int SnapshotsAnalyzed { get; }
    public IReadOnlyList<Anomaly> Anomalies => _anomaliesReadOnly;
    public IReadOnlyList<TopConsumer> TopConsumers => _topConsumersReadOnly;

    public DailyReport(
        DateOnly date,
        DateTimeOffset generatedAt,
        int snapshotsAnalyzed,
        IEnumerable<Anomaly> anomalies,
        IEnumerable<TopConsumer> topConsumers,
        Guid? id = null)
    {
        Guard.AgainstNegative(snapshotsAnalyzed, nameof(snapshotsAnalyzed));
        Guard.AgainstNull(anomalies, nameof(anomalies));
        Guard.AgainstNull(topConsumers, nameof(topConsumers));

        Id = id ?? Guid.NewGuid();
        Date = date;
        GeneratedAt = generatedAt;
        SnapshotsAnalyzed = snapshotsAnalyzed;

        // Детерминированный порядок — сначала критичные, потом по времени.
        _anomalies = anomalies
            .OrderByDescending(a => a.Severity)
            .ThenBy(a => a.OccurredAt)
            .ToList();

        // Сортировка по метрике (CPU → RAM → Disk), затем по значению внутри метрики.
        // AverageValue разных метрик — в разных единицах (%, %, байты/с) — 
        // напрямую сравнивать их нельзя.
        _topConsumers = topConsumers
            .OrderBy(t => t.Metric)
            .ThenByDescending(t => t.AverageValue)
            .ToList();

        _anomaliesReadOnly = _anomalies.AsReadOnly();
        _topConsumersReadOnly = _topConsumers.AsReadOnly();
    }

    public int CriticalAnomalyCount =>
        _anomalies.Count(a => a.Severity == SeverityLevel.Critical);

    public int WarningAnomalyCount =>
        _anomalies.Count(a => a.Severity == SeverityLevel.Warning);

    public Anomaly? GetMostSevereAnomaly() => _anomalies
        .OrderByDescending(a => a.Severity)
        .ThenByDescending(a => a.ObservedValue)
        .FirstOrDefault();

    /// <summary>Топ-потребители по конкретной метрике (уже отсортированные).</summary>
    public IReadOnlyList<TopConsumer> GetTopConsumers(MetricType metric) =>
        _topConsumers.Where(t => t.Metric == metric).ToArray();
}