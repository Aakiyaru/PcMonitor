using PcMonitor.Domain.Entities;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Application.DTOs
{
    /// <summary>
    /// Всё, что нужно анализаторам для работы: снимки за период,
    /// системные события и границы периода.
    /// </summary>
    public sealed record AnalysisContext(
        TimeRange Period,
        IReadOnlyList<ResourceSnapshot> Snapshots,
        IReadOnlyList<SystemEvent> SystemEvents);
}
