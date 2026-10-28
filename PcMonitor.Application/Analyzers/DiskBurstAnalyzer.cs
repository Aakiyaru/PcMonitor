using PcMonitor.Application.Abstractions;
using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Enums;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Application.Analyzers
{
    /// <summary>
    /// Находит эпизоды устойчивой высокой дисковой активности.
    /// Порог сравнивается по суммарному I/O (read + write), но в
    /// причине аномалии отражаются обе компоненты — они диагностически
    /// различны.
    /// </summary>
    public sealed class DiskBurstAnalyzer : IAnomalyAnalyzer
    {
        private static readonly TimeSpan DefaultMinDuration = TimeSpan.FromSeconds(30);

        private readonly MetricThresholds _thresholds;
        private readonly TimeSpan _minDuration;
        private readonly TimeSpan _samplingInterval;

        /// <summary>
        /// Конструктор для production: минимальная длительность — 30 секунд.
        /// </summary>
        public DiskBurstAnalyzer(MetricThresholds thresholds, TimeSpan samplingInterval)
        : this(thresholds, samplingInterval, DefaultMinDuration)
        {
        }

        /// <summary>
        /// Конструктор с явной минимальной длительностью — для тестов
        /// и для сценариев с нестандартной политикой.
        /// </summary>
        public DiskBurstAnalyzer(
            MetricThresholds thresholds,
            TimeSpan samplingInterval,
            TimeSpan minDuration)
        {
            _thresholds = thresholds ?? throw new ArgumentNullException(nameof(thresholds));

            if (samplingInterval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(samplingInterval), "Sampling interval must be positive.");

            if (minDuration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(minDuration), "Minimum episode duration must be positive.");

            _samplingInterval = samplingInterval;
            _minDuration = minDuration;
        }

        public string Name => "disk-burst";

        public Task<IReadOnlyList<Anomaly>> AnalyzeAsync(AnalysisContext context, CancellationToken ct = default)
        {
            var anomalies = new List<Anomaly>();

            if (context.Snapshots.Count == 0) return Task.FromResult<IReadOnlyList<Anomaly>>(anomalies);

            var episode = new List<ResourceSnapshot>();

            foreach (var snapshot in context.Snapshots)
            {
                ct.ThrowIfCancellationRequested();

                if (snapshot.DiskIo.TotalBytesPerSecond >= _thresholds.DiskWarnBytesPerSecond)
                {
                    episode.Add(snapshot);
                }
                else if (episode.Count > 0)
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
            var duration = TimeSpan.FromTicks(_samplingInterval.Ticks * episode.Count);
            if (duration < _minDuration)
                return;

            var peak = episode.MaxBy(s => s.DiskIo.TotalBytesPerSecond)!;
            var peakValue = peak.DiskIo.TotalBytesPerSecond;

            // Classify принимает double; long → double здесь безопасен —
            // значения disk I/O (~10^10 B/s) далеко от предела точности double.
            var severity = _thresholds.Classify(MetricType.DiskIo, peakValue);

            var reason =
                $"Disk I/O above {FormatBytes(_thresholds.DiskWarnBytesPerSecond)}/s " +
                $"for {duration.TotalSeconds:F0}s, " +
                $"peak {FormatBytes(peakValue)}/s " +
                $"(read {FormatBytes(peak.DiskIo.ReadBytesPerSecond)}/s, " +
                $"write {FormatBytes(peak.DiskIo.WriteBytesPerSecond)}/s).";

            output.Add(new Anomaly(
                occurredAt: episode[0].Timestamp,
                metric: MetricType.DiskIo,
                severity: severity,
                reason: reason,
                observedValue: peakValue,
                threshold: _thresholds.DiskWarnBytesPerSecond,
                culprit: peak.GetTopConsumerBy(MetricType.DiskIo)?.Process));
        }

        private static string FormatBytes(long bytes)
        {
            const long KB = 1024L;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            return bytes switch
            {
                >= GB => $"{bytes / (double)GB:F1} GB",
                >= MB => $"{bytes / (double)MB:F1} MB",
                >= KB => $"{bytes / (double)KB:F1} KB",
                _ => $"{bytes} B"
            };
        }
    }
}
