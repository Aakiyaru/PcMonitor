using System.Diagnostics;
using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Infrastructure.DataSources;

public sealed class PerformanceCounterDiskDataSource
    : IDataSource<DiskIoRate>, IDisposable
{
    private readonly PerformanceCounter _read;
    private readonly PerformanceCounter _write;

    public PerformanceCounterDiskDataSource()
    {
        _read = new PerformanceCounter(
            "PhysicalDisk", "Disk Read Bytes/sec", "_Total", readOnly: true);
        _write = new PerformanceCounter(
            "PhysicalDisk", "Disk Write Bytes/sec", "_Total", readOnly: true);

        // Прогрев: скорость считается только между двумя чтениями.
        _ = _read.NextValue();
        _ = _write.NextValue();
    }

    public string Name => "disk-performance-counter";

    public DiskIoRate Read()
    {
        var readBps = Math.Max(0L, (long)_read.NextValue());
        var writeBps = Math.Max(0L, (long)_write.NextValue());
        return new DiskIoRate(readBps, writeBps);
    }

    public void Dispose()
    {
        _read.Dispose();
        _write.Dispose();
    }
}