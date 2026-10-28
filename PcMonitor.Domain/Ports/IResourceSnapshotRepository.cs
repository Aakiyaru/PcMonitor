using PcMonitor.Domain.Entities;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Domain.Ports;

public interface IResourceSnapshotRepository
{
    Task AddAsync(ResourceSnapshot snapshot, CancellationToken ct = default);
    Task<ResourceSnapshot?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ResourceSnapshot>> GetRangeAsync(TimeRange range, CancellationToken ct = default);
    Task<int> CountAsync(TimeRange range, CancellationToken ct = default);
}