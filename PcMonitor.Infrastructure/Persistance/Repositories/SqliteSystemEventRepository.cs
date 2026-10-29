using Microsoft.EntityFrameworkCore;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Ports;
using PcMonitor.Domain.ValueObjects;
using PcMonitor.Infrastructure.Persistance.Mappings;

namespace PcMonitor.Infrastructure.Persistence.Repositories;

public sealed class SqliteSystemEventRepository : ISystemEventRepository
{
    private readonly MonitorDbContext _db;
    public SqliteSystemEventRepository(MonitorDbContext db) => _db = db;

    public async Task<IReadOnlyList<SystemEvent>> GetRangeAsync(
        TimeRange range, CancellationToken ct = default)
    {
        var recs = await _db.SystemEvents
            .AsNoTracking()
            .Where(e => e.Timestamp >= range.From && e.Timestamp < range.To)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(ct);

        return recs.Select(DomainMapper.ToDomain).ToArray();
    }
}