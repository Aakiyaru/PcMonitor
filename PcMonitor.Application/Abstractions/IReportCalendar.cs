using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Application.Abstractions
{
    /// <summary>
    /// Отвечает на вопрос «какой интервал времени соответствует
    /// календарному дню?». Учитывает часовой пояс и переходы DST.
    /// </summary>
    public interface IReportCalendar
    {
        TimeRange GetDayRange(DateOnly date);
    }
}
