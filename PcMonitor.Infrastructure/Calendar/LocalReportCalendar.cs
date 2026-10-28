using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Infrastructure.Calendar
{
    public sealed class LocalReportCalendar : IReportCalendar
    {
        public TimeRange GetDayRange(DateOnly date)
        {
            //смещение берём на "середину дня", чтобы корректно
            //учесть DST-переходы (переход обычно в 2-3 ночи)
            var localMidnight = date.ToDateTime(TimeOnly.MinValue);
            var probe = new DateTime(localMidnight.Year, localMidnight.Month,
                localMidnight.Day, 12, 0, 0, DateTimeKind.Local);
            var offset = TimeZoneInfo.Local.GetUtcOffset(probe);

            return TimeRange.ForDay(date, offset);
        }
    }
}
