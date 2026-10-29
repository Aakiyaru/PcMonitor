using Microsoft.EntityFrameworkCore;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Ports;
using PcMonitor.Infrastructure.Persistance.Mappings;

namespace PcMonitor.Infrastructure.Persistence.Repositories;

public sealed class SqliteDailyReportRepository : IDailyReportRepository
{
    private readonly MonitorDbContext _db;
    public SqliteDailyReportRepository(MonitorDbContext db) => _db = db;

    public async Task SaveAsync(DailyReport report, CancellationToken ct = default)
    {
        var existing = await _db.DailyReports
            .Include(r => r.Anomalies)
            .Include(r => r.TopConsumers)
            .FirstOrDefaultAsync(r => r.Date == report.Date, ct);

        if (existing is null)
        {
            await _db.DailyReports.AddAsync(DomainMapper.ToRecord(report), ct);
            return;
        }

        // Upsert: обновляем скаляры и полностью заменяем дочерние коллекции.
        existing.GeneratedAt = report.GeneratedAt;
        existing.SnapshotsAnalyzed = report.SnapshotsAnalyzed;

        _db.Anomalies.RemoveRange(existing.Anomalies);
        _db.TopConsumers.RemoveRange(existing.TopConsumers);
        existing.Anomalies.Clear();
        existing.TopConsumers.Clear();

        foreach (var a in report.Anomalies)
            existing.Anomalies.Add(DomainMapper.AnomalyRecord(a, existing.Id));
        foreach (var t in report.TopConsumers)
            existing.TopConsumers.Add(DomainMapper.TopConsumerRecord(t, existing.Id));
    }

    public async Task<DailyReport?> GetAsync(DateOnly date, CancellationToken ct = default)
    {
        var rec = await _db.DailyReports
            .AsNoTracking()
            .Include(r => r.Anomalies)
            .Include(r => r.TopConsumers)
            .FirstOrDefaultAsync(r => r.Date == date, ct);

        return rec is null ? null : DomainMapper.ToDomain(rec);
    }

    public async Task<IReadOnlyList<DailyReport>> GetRangeAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var recs = await _db.DailyReports
            .AsNoTracking()
            .Include(r => r.Anomalies)
            .Include(r => r.TopConsumers)
            .Where(r => r.Date >= from && r.Date <= to)
            .OrderBy(r => r.Date)
            .ToListAsync(ct);

        return recs.Select(DomainMapper.ToDomain).ToArray();
    }
}