using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Ports;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Application.Services
{
    public sealed class CollectorOrchestrator : ICollectorOrchestrator
    {
        private readonly IDataSource<CpuUsage> _cpu;
        private readonly IDataSource<MemoryUsage> _memory;
        private readonly IDataSource<DiskIoRate> _disk;
        private readonly IDataSource<GpuMetrics?> _gpu;
        private readonly IDataSource<IReadOnlyList<ProcessSnapshot>> _processes;
        private readonly IClock _clock;

        public CollectorOrchestrator(
        IDataSource<CpuUsage> cpu,
        IDataSource<MemoryUsage> memory,
        IDataSource<DiskIoRate> disk,
        IDataSource<GpuMetrics?> gpu,
        IDataSource<IReadOnlyList<ProcessSnapshot>> processes,
        IClock clock)
        {
            _cpu = cpu;
            _memory = memory;
            _disk = disk;
            _gpu = gpu;
            _processes = processes;
            _clock = clock;
        }

        public Task<ResourceSnapshot> CollectAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var snapshot = new ResourceSnapshot(
                timestamp: _clock.UtcNow,
                cpu: _cpu.Read(),
                memory: _memory.Read(),
                diskIo: _disk.Read(),
                gpu: _gpu.Read(),
                processes: _processes.Read());

            return Task.FromResult(snapshot);
        }
    }
}
