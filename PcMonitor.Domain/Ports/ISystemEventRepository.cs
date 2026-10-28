using PcMonitor.Domain.Entities;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Domain.Ports;

public interface ISystemEventRepository
{
    Task<IReadOnlyList<SystemEvent>> GetRangeAsync(TimeRange range, CancellationToken ct = default);
}