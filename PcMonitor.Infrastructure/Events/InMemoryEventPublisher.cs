using System.Collections.Concurrent;
using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.Events;

namespace PcMonitor.Infrastructure.Events;

public sealed class InMemoryEventPublisher : IEventPublisher
{
    private readonly ConcurrentDictionary<Type, List<Func<DomainEvent, Task>>> _handlers = new();

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
    {
        var handlers = _handlers.GetOrAdd(typeof(TEvent), _ => new List<Func<DomainEvent, Task>>());
        lock (handlers)
            handlers.Add(e => handler((TEvent)e));
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : DomainEvent
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers)) return;

        Func<DomainEvent, Task>[] snapshot;
        lock (handlers) snapshot = handlers.ToArray();

        foreach (var handler in snapshot)
            await handler(@event);
    }
}