using PcMonitor.Application.Abstractions;
using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Enums;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Application.Analyzers;

public sealed class GpuSpikeAnalyzer : IAnomalyAnalyzer
{
    private static readonly TimeSpan DefaultMinDuration = TimeSpan.FromSeconds(30);

    private readonly MetricThresholds _thresholds;
    private readonly TimeSpan _samplingInterval;
    private readonly TimeSpan _minDuration;

    public GpuSpikeAnalyzer(
        MetricThresholds thresholds,
        TimeSpan samplingInterval,
        TimeSpan? minDuration = null)
    {
        _thresholds = thresholds ?? throw new ArgumentNullException(nameof(thresholds));
        if (samplingInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(samplingInterval));
        _samplingInterval = samplingInterval;
        _minDuration = minDuration ?? DefaultMinDuration;
    }

    public string Name => "gpu-spike";

    public Task<IReadOnlyList<Anomaly>> AnalyzeAsync(
        AnalysisContext context, CancellationToken ct = default)
    {
        var anomalies = new List<Anomaly>();
        if (context.Snapshots.Count == 0)
            return Task.FromResult<IReadOnlyList<Anomaly>>(anomalies);

        var episode = new List<ResourceSnapshot>();

        foreach (var snapshot in context.Snapshots)
        {
            ct.ThrowIfCancellationRequested();

            // Снимки без GPU — «дырка» в данных. Эпизод не продолжается,
            // потому что мы не знаем, что было в эти 10 секунд.
            if (snapshot.Gpu is null)
            {
                if (episode.Count > 0)
                {
                    AddIfLongEnough(episode, anomalies);
                    episode = new List<ResourceSnapshot>();
                }
                continue;
            }

            if (snapshot.Gpu.UtilizationPercent >= _thresholds.GpuWarnPercent)
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
        if (duration < _minDuration) return;

        var peak = episode.MaxBy(s => s.Gpu!.UtilizationPercent)!;
        var severity = _thresholds.Classify(MetricType.Gpu, peak.Gpu!.UtilizationPercent);

        var tempText = peak.Gpu.TemperatureCelsius is { } t
            ? $", температура {t:F0}°C"
            : "";

        var reason =
            $"GPU utilization above {_thresholds.GpuWarnPercent:F0}% " +
            $"for {duration.TotalSeconds:F0}s, " +
            $"peak {peak.Gpu.UtilizationPercent:F1}%{tempText}.";

        output.Add(new Anomaly(
            occurredAt: episode[0].Timestamp,
            metric: MetricType.Gpu,
            severity: severity,
            reason: reason,
            observedValue: peak.Gpu.UtilizationPercent,
            threshold: _thresholds.GpuWarnPercent,
            culprit: null));   // GPU-нагрузка не привязана к одному процессу
    }
}