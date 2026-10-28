using PcMonitor.Application.Abstractions;
using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Enums;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Application.Analyzers
{
    public sealed class CpuSpikeAnalyzer : IAnomalyAnalyzer
    {
        private readonly MetricThresholds _thresholds;
        private readonly TimeSpan _samplingInterval;

        public CpuSpikeAnalyzer(MetricThresholds thresholds, TimeSpan samplingInterval)
        {
            _thresholds = thresholds ?? throw new ArgumentNullException(nameof(thresholds));
            if (samplingInterval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(samplingInterval), "Sampling interval must be positive.");
            _samplingInterval = samplingInterval;
        }

        public string Name => "cpu-spike";

        public Task<IReadOnlyList<Anomaly>> AnalyzeAsync(AnalysisContext context, CancellationToken ct = default)
        {
            var anomalies = new List<Anomaly>();

            if (context.Snapshots.Count == 0) return Task.FromResult<IReadOnlyList<Anomaly>>(anomalies);

            //группируем последовательные снимки с CPU выше warning в эпизоды
            var episode = new List<ResourceSnapshot>();

            foreach (var snapshot in context.Snapshots)
            {
                ct.ThrowIfCancellationRequested();
                var threshold = _thresholds.CpuWarnPercent;

                if(snapshot.Cpu.Percent >= threshold)
                {
                    episode.Add(snapshot);
                }
                else if(episode.Count > 0)
                {
                    anomalies.Add(BuildEpisodeAnomaly(episode));
                    episode = new List<ResourceSnapshot>();
                }
            }

            if (episode.Count > 0)
                anomalies.Add(BuildEpisodeAnomaly(episode));

            return Task.FromResult<IReadOnlyList<Anomaly>>(anomalies);
        }

        private Anomaly BuildEpisodeAnomaly(IReadOnlyList<ResourceSnapshot> episode)
        {
            var peak = episode.MaxBy(s => s.Cpu.Percent)!;
            var severity = _thresholds.Classify(MetricType.Cpu, peak.Cpu.Percent);

            var duration = TimeSpan.FromTicks(_samplingInterval.Ticks * episode.Count);
            var durationSeconds = (int)duration.TotalSeconds;

            var reason = $"CPU sustained above {_thresholds.CpuWarnPercent:F0}% " +
                $"for {durationSeconds}s, peak {peak.Cpu.Percent:F1}%";

            return new Anomaly(
                occurredAt: episode[0].Timestamp,
                metric: MetricType.Cpu,
                severity: severity,
                reason: reason,
                observedValue: peak.Cpu.Percent,
                threshold: _thresholds.CpuWarnPercent,
                culprit: peak.GetTopConsumerBy(MetricType.Cpu)?.Process);
        }
    }
}
