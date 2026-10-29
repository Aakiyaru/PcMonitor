namespace PcMonitor.Infrastructure.Persistence.Entities;

public sealed class TopConsumerRecord
{
    public long Id { get; set; }
    public Guid DailyReportId { get; set; }

    public int ProcessPid { get; set; }
    public string ProcessName { get; set; } = null!;
    public int Metric { get; set; }
    public double AverageValue { get; set; }
    public double PeakValue { get; set; }

    // TimeSpan — EF Core 8 SQLite хранит как TEXT. Сортировка по нему не нужна.
    public TimeSpan TotalObservedTime { get; set; }

    public DailyReportRecord Report { get; set; } = null!;
}