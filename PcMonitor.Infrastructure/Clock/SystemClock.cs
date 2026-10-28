using PcMonitor.Domain.Ports;

namespace PcMonitor.Infrastructure.Clock;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}