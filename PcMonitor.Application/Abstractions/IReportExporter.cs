using PcMonitor.Domain.Entities;

namespace PcMonitor.Application.Abstractions;

public interface IReportExporter
{
    Task ExportAsync(
        DailyReport report,
        IReadOnlyList<ResourceSnapshot> snapshots,
        CancellationToken ct = default);
}