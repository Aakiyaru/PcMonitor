using PcMonitor.Domain.Enums;

namespace PcMonitor.Domain.ValueObjects;

/// <summary>
/// Бизнес-политика порогов. Immutable-конфигурация, передаётся
/// в анализаторы Application-слоя. Значения по умолчанию покрывают
/// типовые сценарии десктопа.
/// </summary>
public sealed record MetricThresholds
{
    public double CpuWarnPercent { get; init; } = 70;
    public double CpuCriticalPercent { get; init; } = 90;
    public double MemoryWarnPercent { get; init; } = 75;
    public double MemoryCriticalPercent { get; init; } = 90;
    public long DiskWarnBytesPerSecond { get; init; } = 50L * 1024 * 1024;
    public long DiskCriticalBytesPerSecond { get; init; } = 200L * 1024 * 1024;
    public double GpuWarnPercent { get; init; } = 80;
    public double GpuCriticalPercent { get; init; } = 95;

    public SeverityLevel Classify(MetricType metric, double value) => metric switch
    {
        MetricType.Cpu => value >= CpuCriticalPercent ? SeverityLevel.Critical
                        : value >= CpuWarnPercent ? SeverityLevel.Warning
                        : SeverityLevel.Info,
        MetricType.Memory => value >= MemoryCriticalPercent ? SeverityLevel.Critical
                           : value >= MemoryWarnPercent ? SeverityLevel.Warning
                           : SeverityLevel.Info,
        MetricType.DiskIo => value >= DiskCriticalBytesPerSecond ? SeverityLevel.Critical
                           : value >= DiskWarnBytesPerSecond ? SeverityLevel.Warning
                           : SeverityLevel.Info,
        MetricType.Gpu => value >= GpuCriticalPercent ? SeverityLevel.Critical
                        : value >= GpuWarnPercent ? SeverityLevel.Warning
                        : SeverityLevel.Info,
        _ => SeverityLevel.Info
    };
}