using PcMonitor.Domain.Common;

namespace PcMonitor.Domain.ValueObjects;

/// <summary>Полуоткрытый интервал [From, To).</summary>
public readonly record struct TimeRange
{
    public DateTimeOffset From { get; }
    public DateTimeOffset To { get; }

    public TimeRange(DateTimeOffset from, DateTimeOffset to)
    {
        if (to <= from)
            throw new DomainException("To must be strictly after From.");
        From = from;
        To = to;
    }

    public TimeSpan Duration => To - From;
    public bool Contains(DateTimeOffset point) => point >= From && point < To;

    /// <summary>
    /// Границы календарного дня в заданном смещении (обычно —
    /// локальное смещение системы, передаётся из Infrastructure).
    /// </summary>
    public static TimeRange ForDay(DateOnly day, TimeSpan offset) => new(
        new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), offset),
        new DateTimeOffset(day.AddDays(1).ToDateTime(TimeOnly.MinValue), offset));
}