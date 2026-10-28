using PcMonitor.Domain.Common;

namespace PcMonitor.Domain.ValueObjects;

public readonly record struct CpuUsage
{
    public double Percent { get; }

    public CpuUsage(double percent)
    {
        Guard.AgainstOutOfRange(percent, 0, 100, nameof(percent));
        Percent = percent;
    }

    public static CpuUsage Zero => new(0);

    public bool Exceeds(double threshold) => Percent > threshold;
}