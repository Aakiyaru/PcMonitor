using PcMonitor.Domain.Events;

namespace PcMonitor.Application.Abstractions
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
            where TEvent : DomainEvent;
    }
}
