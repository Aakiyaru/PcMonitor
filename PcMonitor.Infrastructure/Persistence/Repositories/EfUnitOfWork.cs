using PcMonitor.Domain.Ports;

namespace PcMonitor.Infrastructure.Persistence.Repositories;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly MonitorDbContext _db;
    public EfUnitOfWork(MonitorDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}