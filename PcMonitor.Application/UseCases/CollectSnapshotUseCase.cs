using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.Events;
using PcMonitor.Domain.Ports;

namespace PcMonitor.Application.UseCases;

public sealed class CollectSnapshotUseCase
{
    private readonly ICollectorOrchestrator _collector;
    private readonly IResourceSnapshotRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _events;

    public CollectSnapshotUseCase(
        ICollectorOrchestrator collector,
        IResourceSnapshotRepository repository,
        IUnitOfWork unitOfWork,
        IEventPublisher events)
    {
        _collector = collector;
        _repository = repository;
        _unitOfWork = unitOfWork;
        _events = events;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        // 1. Собрать снимок
        var snapshot = await _collector.CollectAsync(ct);

        // 2. Сохранить
        await _repository.AddAsync(snapshot, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // 3. Опубликовать событие
        await _events.PublishAsync(
            new SnapshotCollected(
                snapshot.Id,
                snapshot.Timestamp,
                snapshot.Cpu,
                snapshot.Memory),
            ct);
    }
}