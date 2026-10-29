using PcMonitor.Domain.Entities;

namespace PcMonitor.Domain.Ports;

public interface IDailyReportRepository
{
    Task SaveAsync(DailyReport report, CancellationToken ct = default);

    Task<DailyReport?> GetAsync(
        DateOnly reportDate,
        CancellationToken ct = default);

    Task<IReadOnlyList<DailyReport>> GetRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct = default);
}