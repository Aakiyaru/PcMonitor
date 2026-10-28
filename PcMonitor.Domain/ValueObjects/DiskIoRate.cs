using PcMonitor.Domain.Common;

namespace PcMonitor.Domain.ValueObjects;

/// <summary>Скорость дискового ввода-вывода в байтах в секунду.</summary>
public readonly record struct DiskIoRate
{
    public long ReadBytesPerSecond { get; }
    public long WriteBytesPerSecond { get; }

    public DiskIoRate(long readBytesPerSecond, long writeBytesPerSecond)
    {
        Guard.AgainstNegative(readBytesPerSecond, nameof(readBytesPerSecond));
        Guard.AgainstNegative(writeBytesPerSecond, nameof(writeBytesPerSecond));
        ReadBytesPerSecond = readBytesPerSecond;
        WriteBytesPerSecond = writeBytesPerSecond;
    }

    public long TotalBytesPerSecond => ReadBytesPerSecond + WriteBytesPerSecond;

    public static DiskIoRate Zero => new(0, 0);
}