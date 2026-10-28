using PcMonitor.Domain.Common;

namespace PcMonitor.Domain.ValueObjects;

/// <summary>Память, занятая конкретным процессом (working set).</summary>
public readonly record struct ProcessMemoryUsage
{
    public long WorkingSetBytes { get; }

    public ProcessMemoryUsage(long workingSetBytes)
    {
        Guard.AgainstNegative(workingSetBytes, nameof(workingSetBytes));
        WorkingSetBytes = workingSetBytes;
    }

    public static ProcessMemoryUsage Zero => new(0);
}