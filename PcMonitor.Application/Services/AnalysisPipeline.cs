using PcMonitor.Application.Abstractions;
using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;

namespace PcMonitor.Application.Services
{
    public sealed class AnalysisPipeline : IAnalysisPipeline
    {
        private readonly IEnumerable<IAnomalyAnalyzer> _analyzers;
        private readonly IEnumerable<IAnomalyEnricher> _enrichers;

        public AnalysisPipeline(
            IEnumerable<IAnomalyAnalyzer> analyzers,
            IEnumerable<IAnomalyEnricher> enrichers)
        {
            _analyzers = analyzers;
            _enrichers = enrichers;
        }

        public async Task<IReadOnlyList<Anomaly>> RunAsync(
            AnalysisContext context, CancellationToken ct = default)
        {
            var all = new List<Anomaly>();

            // Фаза 1: независимые анализаторы.
            foreach (var analyzer in _analyzers)
            {
                var found = await analyzer.AnalyzeAsync(context, ct);
                all.AddRange(found);
            }

            // Фаза 2: обогащение — получает уже накопленный результат.
            IReadOnlyList<Anomaly> current = all;
            foreach (var enricher in _enrichers)
            {
                current = await enricher.EnrichAsync(current, context, ct);
            }

            // Финальный порядок: сначала критичные, потом по времени.
            return current
                .OrderByDescending(a => a.Severity)
                .ThenBy(a => a.OccurredAt)
                .ToArray();
        }
    }
}
