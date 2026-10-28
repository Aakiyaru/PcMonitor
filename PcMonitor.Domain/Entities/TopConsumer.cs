using PcMonitor.Domain.Common;
using PcMonitor.Domain.Enums;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Domain.Entities;

/// <summary>
/// Сводка по процессу — «топ-потребитель» метрики за период.
/// </summary>
public sealed record TopConsumer
{
    public ProcessIdentifier Process { get; init; }
    public MetricType Metric { get; init; }
    public double AverageValue { get; init; }
    public double PeakValue { get; init; }
    public TimeSpan TotalObservedTime { get; init; }

    public TopConsumer(
        ProcessIdentifier process,
        MetricType metric,
        double averageValue,
        double peakValue,
        TimeSpan totalObservedTime)
    {
        Guard.AgainstNegative(averageValue, nameof(averageValue));
        Guard.AgainstNegative(peakValue, nameof(peakValue));
        if (peakValue < averageValue)
            throw new DomainException("Peak cannot be less than average.");
        if (totalObservedTime < TimeSpan.Zero)
            throw new DomainException("TotalObservedTime must be non-negative.");

        Process = process;
        Metric = metric;
        AverageValue = averageValue;
        PeakValue = peakValue;
        TotalObservedTime = totalObservedTime;
    }
}