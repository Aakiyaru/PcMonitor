namespace PcMonitor.Domain.Events;

public sealed record DailyReportGenerated(
    Guid ReportId,
    DateOnly Date,
    int AnomalyCount,
    DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);