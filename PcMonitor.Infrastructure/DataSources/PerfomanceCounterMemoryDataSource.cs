using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.ValueObjects;
using System.Diagnostics;

namespace PcMonitor.Infrastructure.DataSources;

public sealed class PerformanceCounterMemoryDataSource
    : IDataSource<MemoryUsage>
{
    public string Name => "memory-performance-counter";

    public MemoryUsage Read()
    {
        // Available MBytes — самый надёжный счётчик памяти,
        // есть на всех версиях Windows без доп. настроек.
        using var counter = new PerformanceCounter(
            "Memory", "Available MBytes", readOnly: true);

        var availableMb = counter.NextValue();
        var availableBytes = (long)(availableMb * 1024 * 1024);

        // Общий объём RAM получаем через GlobalMemoryStatusEx
        var totalBytes = GetTotalPhysicalMemory();

        var usedBytes = totalBytes - availableBytes;
        return new MemoryUsage(usedBytes, totalBytes);
    }

    private static long GetTotalPhysicalMemory()
    {
        var info = new MEMORYSTATUSEX { dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf<MEMORYSTATUSEX>() };
        if (!GlobalMemoryStatusEx(ref info))
            throw new InvalidOperationException("GlobalMemoryStatusEx failed.");
        return (long)info.ullTotalPhys;
    }

    [System.Runtime.InteropServices.StructLayout(
        System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(
        System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
}