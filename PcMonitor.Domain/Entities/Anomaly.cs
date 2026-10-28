using PcMonitor.Domain.Common;
using PcMonitor.Domain.Enums;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Domain.Entities;

/// <summary>
/// Зафиксированная аномалия в потреблении ресурсов.
/// </summary>
public sealed record Anomaly
{
    public DateTimeOffset OccurredAt { get; init; }
    public MetricType Metric { get; init; }
    public SeverityLevel Severity { get; init; }
    public string Reason { get; init; }
    public double ObservedValue { get; init; }
    public double? Threshold { get; init; }
    public ProcessIdentifier? Culprit { get; init; }

    public Anomaly(
        DateTimeOffset occurredAt,
        MetricType metric,
        SeverityLevel severity,
        string reason,
        double observedValue,
        double? threshold = null,
        ProcessIdentifier? culprit = null)
    {
        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason));
        Guard.AgainstNegative(observedValue, nameof(observedValue));
        if (threshold is { } t) Guard.AgainstNegative(t, nameof(threshold));

        OccurredAt = occurredAt;
        Metric = metric;
        Severity = severity;
        Reason = reason;
        ObservedValue = observedValue;
        Threshold = threshold;
        Culprit = culprit;
    }
}