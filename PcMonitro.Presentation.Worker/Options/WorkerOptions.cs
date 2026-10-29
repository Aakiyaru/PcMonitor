namespace PcMonitor.Presentation.Worker.Options;

public sealed class WorkerOptions
{
    public int SamplingIntervalSeconds { get; set; } = 10;

    public double CpuWarnPercent { get; set; } = 70;
    public double CpuCriticalPercent { get; set; } = 90;
    public double MemoryWarnPercent { get; set; } = 75;
    public double MemoryCriticalPercent { get; set; } = 90;
    public long DiskWarnMegabytesPerSecond { get; set; } = 50;
    public long DiskCriticalMegabytesPerSecond { get; set; } = 200;
    public double GpuWarnPercent { get; set; } = 80;
    public double GpuCriticalPercent { get; set; } = 95;

    public int ReportCheckIntervalSeconds { get; set; } = 900;
    public int RetentionDays { get; set; } = 0;
}