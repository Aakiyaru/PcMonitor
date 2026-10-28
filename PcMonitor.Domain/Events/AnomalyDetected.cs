using PcMonitor.Domain.Enums;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Domain.Events;

public sealed record AnomalyDetected(
    MetricType Metric,
    SeverityLevel Severity,
    string Reason,
    double ObservedValue,
    ProcessIdentifier? Culprit,
    DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);