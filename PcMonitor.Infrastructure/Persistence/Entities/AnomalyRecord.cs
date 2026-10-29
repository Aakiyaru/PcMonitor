namespace PcMonitor.Infrastructure.Persistence.Entities;

public sealed class AnomalyRecord
{
    public long Id { get; set; }               // автоинкремент (PK)
    public Guid DailyReportId { get; set; }    // FK на DailyReports

    public DateTimeOffset OccurredAt { get; set; }
    public int Metric { get; set; }            // MetricType как int
    public int Severity { get; set; }          // SeverityLevel как int
    public string Reason { get; set; } = null!;
    public double ObservedValue { get; set; }
    public double? Threshold { get; set; }

    // ProcessIdentifier? — раскладываем на два nullable-поля.
    // Инвариант: либо оба null (системная аномалия), либо оба заполнены.
    public int? CulpritPid { get; set; }
    public string? CulpritName { get; set; }

    public DailyReportRecord Report { get; set; } = null!;
}