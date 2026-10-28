using PcMonitor.Domain.Entities;

namespace PcMonitor.Domain.Ports;

public interface IDailyReportRepository
{
    Task SaveAsync(DailyReport report, CancellationToken ct = default);
    Task<DailyReport?> GetAsync(DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<DailyReport>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}