namespace PcMonitor.Domain.Ports;

/// <summary>Абстракция времени — позволяет подменять часы в тестах.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}