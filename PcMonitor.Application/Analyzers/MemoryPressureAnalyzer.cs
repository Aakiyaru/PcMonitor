using PcMonitor.Application.Abstractions;
using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Enums;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Application.Analyzers
{
    public sealed class MemoryPressureAnalyzer : IAnomalyAnalyzer
    {
        private readonly MetricThresholds _thresholds;
        private readonly TimeSpan _minDuration;
        private readonly TimeSpan _samplingInterval;

        public MemoryPressureAnalyzer(
            MetricThresholds thresholds,
            TimeSpan samplingInterval,
            TimeSpan? minDuration = null)
        {
            _thresholds = thresholds;
            _samplingInterval = samplingInterval;
            _minDuration = minDuration ?? TimeSpan.FromMinutes(5);
        }
        public string Name => "memory-pressure";

        public Task<IReadOnlyList<Anomaly>> AnalyzeAsync(AnalysisContext context, CancellationToken ct = default)
        {
            var anomalies = new List<Anomaly>();

            if (context.Snapshots.Count == 0) return Task.FromResult<IReadOnlyList<Anomaly>>(anomalies);

            var episode = new List<ResourceSnapshot>();

            foreach (var snapshot in context.Snapshots)
            {
                ct.ThrowIfCancellationRequested();
                if (snapshot.Memory.UtilizationPercent >= _thresholds.MemoryWarnPercent)
                    episode.Add(snapshot);
                else if(episode.Count > 0)
                {
                    AddIfLongEnough(episode, anomalies);
                    episode = new List<ResourceSnapshot>();
                }
            }

            if (episode.Count > 0)
                AddIfLongEnough(episode, anomalies);

            return Task.FromResult<IReadOnlyList<Anomaly>>(anomalies);
        }

        private void AddIfLongEnough(IReadOnlyList<ResourceSnapshot> episode, List<Anomaly> output)
        {
            //Пиковое значение + длительность
            var peak = episode.MaxBy(s => s.Memory.UtilizationPercent)!;
            var duration = TimeSpan.FromTicks(_samplingInterval.Ticks * episode.Count);

            //для памяти важен факт устойчивости, а не кратковременный всплеск
            if (duration < _minDuration) return;

            var severity = _thresholds.Classify(MetricType.Memory, peak.Memory.UtilizationPercent);
            var reason = $"Memory utilization above {_thresholds.MemoryWarnPercent:F0}% " +
                     $"for {duration.TotalMinutes:F0} min, peak {peak.Memory.UtilizationPercent:F1}%.";

            output.Add(new Anomaly(
            occurredAt: episode[0].Timestamp,
            metric: MetricType.Memory,
            severity: severity,
            reason: reason,
            observedValue: peak.Memory.UtilizationPercent,
            threshold: _thresholds.MemoryWarnPercent,
            culprit: peak.GetTopConsumerBy(MetricType.Memory)?.Process));
        }
    }
}
