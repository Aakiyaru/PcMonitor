using PcMonitor.Domain.Common;

namespace PcMonitor.Domain.ValueObjects;

/// <summary>Использование системной памяти.</summary>
public readonly record struct MemoryUsage
{
    public long UsedBytes { get; }
    public long TotalBytes { get; }

    public MemoryUsage(long usedBytes, long totalBytes)
    {
        Guard.AgainstNegativeOrZero(totalBytes, nameof(totalBytes));
        if (usedBytes < 0 || usedBytes > totalBytes)
            throw new DomainException("UsedBytes must be in [0, TotalBytes].");

        UsedBytes = usedBytes;
        TotalBytes = totalBytes;
    }

    public long AvailableBytes => TotalBytes - UsedBytes;
    public double UtilizationPercent => (double)UsedBytes / TotalBytes * 100;

    public static MemoryUsage Empty => new(0, 1); // 1 byte total — минимальный валидный
}