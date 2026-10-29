namespace PcMonitor.Infrastructure.Persistence.Entities;

public sealed class DailyReportRecord
{
    public Guid Id { get; set; }

    // DateOnly — EF Core 8+ SQLite поддерживает нативно (TEXT "yyyy-MM-dd").
    // На EF Core 7 — добавьте HasConversion<DateOnlyConverter> в OnModelCreating.
    public DateOnly Date { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }
    public int SnapshotsAnalyzed { get; set; }

    // Дочерние коллекции
    public List<AnomalyRecord> Anomalies { get; set; } = new();
    public List<TopConsumerRecord> TopConsumers { get; set; } = new();
}