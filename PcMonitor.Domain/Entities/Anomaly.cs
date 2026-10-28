using PcMonitor.Domain.Common;
using PcMonitor.Domain.Enums;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Domain.Entities;

/// <summary>
/// Зафиксированная аномалия в потреблении ресурсов.
/// </summary>
public sealed record Anomaly
{
    public DateTimeOffset OccurredAt { get; }
    public MetricType Metric { get; }
    public SeverityLevel Severity { get; }
    public string Reason { get; }
    public double ObservedValue { get; }
    public double? Threshold { get; }
    public ProcessIdentifier? Culprit { get; }

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