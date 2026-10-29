namespace PcMonitor.Infrastructure.Persistence.Entities;

public sealed class SnapshotRecord
{
    public Guid Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    // CPU
    public double CpuPercent { get; set; }

    // Memory
    public long MemoryUsedBytes { get; set; }
    public long MemoryTotalBytes { get; set; }

    // Disk
    public long DiskReadBps { get; set; }
    public long DiskWriteBps { get; set; }

    // GPU (nullable — на машинах без GPU)
    public string? GpuDeviceName { get; set; }
    public double? GpuUtilizationPercent { get; set; }
    public double? GpuTemperatureCelsius { get; set; }
    public long? GpuMemoryUsedBytes { get; set; }
    public long? GpuMemoryTotalBytes { get; set; }

    // Навигация
    public List<ProcessRecord> Processes { get; set; } = new();
}