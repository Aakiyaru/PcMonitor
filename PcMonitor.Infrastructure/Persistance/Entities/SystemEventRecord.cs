namespace PcMonitor.Infrastructure.Persistence.Entities;

public sealed class SystemEventRecord
{
    public Guid Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    // SystemEventCategory — хранится как int (значения зафиксированы в Domain)
    public int Category { get; set; }

    public string Source { get; set; } = null!;
    public string Message { get; set; } = null!;

    // SeverityLevel — как int
    public int Severity { get; set; }
}