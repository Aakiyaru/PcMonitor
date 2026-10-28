using System.Diagnostics;
using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Infrastructure.DataSources;

public sealed class ProcessDataSource : IDataSource<IReadOnlyList<ProcessSnapshot>>
{
    private readonly Dictionary<int, ProcessSample> _previous = new();
    private readonly object _gate = new();
    private DateTimeOffset _lastSampleTime;

    public string Name => "processes";

    public IReadOnlyList<ProcessSnapshot> Read()
    {
        var now = DateTimeOffset.UtcNow;
        var result = new List<ProcessSnapshot>();
        var currentPids = new HashSet<int>();

        foreach (var p in Process.GetProcesses())
        {
            using (p)
            {
                if (p.Id <= 0) continue;   // System Idle Process
                currentPids.Add(p.Id);

                try
                {
                    var name = p.ProcessName;
                    var cpu = ComputeCpu(p.Id, p.TotalProcessorTime, now);
                    var mem = new ProcessMemoryUsage(p.WorkingSet64);
                    var disk = ComputeDisk(p.Id, p, now);

                    result.Add(new ProcessSnapshot(
                        new ProcessIdentifier(p.Id, name), cpu, mem, disk));
                }
                catch (Exception ex) when (
                    ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                    // Процесс завершился между GetProcesses и чтением — нормально.
                    // Пропускаем.
                }
            }
        }

        // Очищаем память о завершившихся процессах
        lock (_gate)
        {
            var toRemove = _previous.Keys.Except(currentPids).ToList();
            foreach (var pid in toRemove) _previous.Remove(pid);
            _lastSampleTime = now;
        }

        return result;
    }

    private CpuUsage ComputeCpu(int pid, TimeSpan totalProcessorTime, DateTimeOffset now)
    {
        lock (_gate)
        {
            if (!_previous.TryGetValue(pid, out var prev))
            {
                _previous[pid] = new ProcessSample(totalProcessorTime, now, 0, 0);
                return CpuUsage.Zero;  // первое наблюдение — нет данных для delta
            }

            var elapsed = (now - prev.Timestamp).TotalMilliseconds;
            if (elapsed <= 0) return CpuUsage.Zero;

            var cpuMs = (totalProcessorTime - prev.CpuTime).TotalMilliseconds;
            var cores = Environment.ProcessorCount;
            var percent = cpuMs / (elapsed * cores) * 100.0;

            _previous[pid] = prev with { CpuTime = totalProcessorTime, Timestamp = now };
            return new CpuUsage(Math.Clamp(percent, 0, 100));
        }
    }

    private DiskIoRate ComputeDisk(int pid, Process p, DateTimeOffset now)
    {
        try
        {
            var read = GetProcessIo(pid).ReadBytes;
            var write = GetProcessIo(pid).ReadBytes;

            lock (_gate)
            {
                if (!_previous.TryGetValue(pid, out var prev) || prev.LastReadBytes == 0)
                {
                    _previous[pid] = prev is null
                        ? new ProcessSample(p.TotalProcessorTime, now, read, write)
                        : prev with { LastReadBytes = read, LastWriteBytes = write };
                    return DiskIoRate.Zero;
                }

                var elapsedSec = (now - prev.Timestamp).TotalSeconds;
                if (elapsedSec <= 0) return DiskIoRate.Zero;

                var readBps = Math.Max(0L, (long)((read - prev.LastReadBytes) / elapsedSec));
                var writeBps = Math.Max(0L, (long)((write - prev.LastWriteBytes) / elapsedSec));

                _previous[pid] = prev with { LastReadBytes = read, LastWriteBytes = write };
                return new DiskIoRate(readBps, writeBps);
            }
        }
        catch
        {
            return DiskIoRate.Zero;
        }
    }

    private record ProcessSample(
        TimeSpan CpuTime,
        DateTimeOffset Timestamp,
        long LastReadBytes,
        long LastWriteBytes);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetProcessIoCounters(
    IntPtr hProcess, out IO_COUNTERS counters);

    [System.Runtime.InteropServices.StructLayout(
        System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    private static (long ReadBytes, long WriteBytes) GetProcessIo(int pid)
    {
        var handle = OpenProcess(0x0400 /* PROCESS_QUERY_INFORMATION */, false, pid);
        if (handle == IntPtr.Zero) return (0, 0);
        try
        {
            if (!GetProcessIoCounters(handle, out var c)) return (0, 0);
            return ((long)c.ReadTransferCount, (long)c.WriteTransferCount);
        }
        finally { CloseHandle(handle); }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint access, bool inherit, int pid);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);
}