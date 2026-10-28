using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Domain.Events;

public sealed record SnapshotCollected(
    Guid SnapshotId,
    DateTimeOffset CollectedAt,
    CpuUsage Cpu,
    MemoryUsage Memory) : DomainEvent(CollectedAt);