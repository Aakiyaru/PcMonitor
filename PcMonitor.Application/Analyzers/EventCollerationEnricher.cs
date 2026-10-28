using PcMonitor.Application.Abstractions;
using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;

namespace PcMonitor.Application.Analyzers
{
    public sealed class EventCollerationEnricher : IAnomalyEnricher
    {
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(2);

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

                //Ищем событие в окне ±Window от момента аномалии
                var nearby = context.SystemEvents
                    .Where(e => Math.Abs((e.Timestamp - anomaly.OccurredAt).TotalSeconds)
                        <= Window.TotalSeconds)
                    .OrderByDescending(e => e.Severity)
                    .FirstOrDefault();

                if(nearby is  null)
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
}
