using PcMonitor.Domain.Entities;

namespace PcMonitor.Application.Abstractions
{
    public interface ICollectorOrchestrator
    {
        Task<ResourceSnapshot> CollectAsync(CancellationToken ct = default);
    }
}
