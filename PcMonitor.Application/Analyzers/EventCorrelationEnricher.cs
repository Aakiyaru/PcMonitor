using PcMonitor.Application.Abstractions;
using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Enums;

namespace PcMonitor.Application.Analyzers;

/// <summary>
/// Дополняет уже найденные аномалии контекстом: ищет системные
/// события (антивирус, Windows Update, драйвер) в окне ±2 минуты
/// от момента аномалии и вписывает самое серьёзное в Reason.
/// </summary>
public sealed class EventCorrelationEnricher : IAnomalyEnricher
{
    private static readonly TimeSpan CorrelationWindow = TimeSpan.FromMinutes(2);

    public Task<IReadOnlyList<Anomaly>> EnrichAsync(
        IReadOnlyList<Anomaly> anomalies,
        AnalysisContext context,
        CancellationToken ct = default)
    {
        if (anomalies.Count == 0 || context.SystemEvents.Count == 0)
            return Task.FromResult(anomalies);

        var enriched = new List<Anomaly>(anomalies.Count);

        foreach (var anomaly in anomalies)
        {
            ct.ThrowIfCancellationRequested();

            var nearby = context.SystemEvents
                .Where(e => Math.Abs((e.Timestamp - anomaly.OccurredAt).TotalSeconds)
                            <= CorrelationWindow.TotalSeconds)
                .OrderByDescending(e => e.Severity)
                .FirstOrDefault();

            if (nearby is null)
            {
                enriched.Add(anomaly);
                continue;
            }

            var enrichedReason = $"{anomaly.Reason} " +
                                 $"Correlated with system event [{nearby.Category}] " +
                                 $"'{nearby.Source}': {nearby.Message}";

            enriched.Add(anomaly with { Reason = enrichedReason });
        }

        return Task.FromResult<IReadOnlyList<Anomaly>>(enriched);
    }
}