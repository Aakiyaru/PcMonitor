using PcMonitor.Domain.Common;

namespace PcMonitor.Domain.ValueObjects;

/// <summary>
/// Метрики одной GPU. Сенсоры опциональны — у разных вендоров
/// доступен разный набор.
/// </summary>
public sealed record GpuMetrics
{
    public string DeviceName { get; }
    public double UtilizationPercent { get; }
    public double? TemperatureCelsius { get; }
    public long? MemoryUsedBytes { get; }
    public long? MemoryTotalBytes { get; }

    public GpuMetrics(
        string deviceName,
        double utilizationPercent,
        double? temperatureCelsius = null,
        long? memoryUsedBytes = null,
        long? memoryTotalBytes = null)
    {
        Guard.AgainstNullOrWhiteSpace(deviceName, nameof(deviceName));
        Guard.AgainstOutOfRange(utilizationPercent, 0, 100, nameof(utilizationPercent));

        if (temperatureCelsius is { } t)
            Guard.AgainstOutOfRange(t, -40, 150, nameof(temperatureCelsius));

        if (memoryUsedBytes is { } used) Guard.AgainstNegative(used, nameof(memoryUsedBytes));
        if (memoryTotalBytes is { } total) Guard.AgainstNegative(total, nameof(memoryTotalBytes));

        if (memoryUsedBytes is { } mu && memoryTotalBytes is { } mt && mu > mt)
            throw new DomainException("MemoryUsedBytes cannot exceed MemoryTotalBytes.");

        DeviceName = deviceName;
        UtilizationPercent = utilizationPercent;
        TemperatureCelsius = temperatureCelsius;
        MemoryUsedBytes = memoryUsedBytes;
        MemoryTotalBytes = memoryTotalBytes;
    }
}