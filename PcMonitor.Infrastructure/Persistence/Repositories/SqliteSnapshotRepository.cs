using Microsoft.EntityFrameworkCore;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Ports;
using PcMonitor.Domain.ValueObjects;
using PcMonitor.Infrastructure.Persistence.Mappings;

namespace PcMonitor.Infrastructure.Persistence.Repositories;

public sealed class SqliteSnapshotRepository : IResourceSnapshotRepository
{
    private readonly MonitorDbContext _db;
    public SqliteSnapshotRepository(MonitorDbContext db) => _db = db;

    public async Task AddAsync(ResourceSnapshot s, CancellationToken ct = default)
    {
        var record = DomainMapper.ToRecord(s);
        await _db.Snapshots.AddAsync(record, ct);
    }

    public async Task<ResourceSnapshot?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var rec = await _db.Snapshots
            .AsNoTracking()
            .Include(s => s.Processes)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
        return rec is null ? null : DomainMapper.ToDomain(rec);
    }

    public async Task<IReadOnlyList<ResourceSnapshot>> GetRangeAsync(
        TimeRange range, CancellationToken ct = default)
    {
        var recs = await _db.Snapshots
            .AsNoTracking()
            .Include(s => s.Processes)
            .Where(s => s.Timestamp >= range.From && s.Timestamp < range.To)
            .OrderBy(s => s.Timestamp)
            .ToListAsync(ct);

        return recs.Select(DomainMapper.ToDomain).ToArray();
    }

    public Task<int> CountAsync(TimeRange range, CancellationToken ct = default)
        => _db.Snapshots.CountAsync(
            s => s.Timestamp >= range.From && s.Timestamp < range.To, ct);
}