namespace PcMonitor.Domain.Events;

/// <summary>
/// Базовый тип доменного события. Публикуется Application-слоем.
/// Домен не хранит события в агрегате — это делает use case.
/// </summary>
public abstract record DomainEvent(DateTimeOffset OccurredAt);