using PcMonitor.Domain.Common;
using PcMonitor.Domain.Enums;

namespace PcMonitor.Domain.Entities;

/// <summary>
/// Запись из журнала событий ОС (Event Log, WMI, кастомные источники).
/// Используется для корреляции всплесков нагрузки с системными событиями.
/// </summary>
public sealed class SystemEvent
{
    public Guid Id { get; }
    public DateTimeOffset Timestamp { get; }
    public SystemEventCategory Category { get; }
    public string Source { get; }
    public string Message { get; }
    public SeverityLevel Severity { get; }

    public SystemEvent(
        DateTimeOffset timestamp,
        SystemEventCategory category,
        string source,
        string message,
        SeverityLevel severity = SeverityLevel.Info,
        Guid? id = null)
    {
        Guard.AgainstNullOrWhiteSpace(source, nameof(source));
        Guard.AgainstNullOrWhiteSpace(message, nameof(message));

        Id = id ?? Guid.NewGuid();
        Timestamp = timestamp;
        Category = category;
        Source = source;
        Message = message;
        Severity = severity;
    }
}