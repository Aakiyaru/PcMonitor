using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Enums;

namespace PcMonitor.Infrastructure.Reporting;

public static class DailyReportChartFactory
{
    public static ChartRequest CpuTimeline(IReadOnlyList<ResourceSnapshot> snapshots)
    {
        var points = snapshots
            .Select((s, i) => new ChartPoint(i, s.Cpu.Percent))
            .ToArray();

        return new ChartRequest(
            Kind: ChartKind.Line,
            Title: "CPU за сутки",
            Series: new[] { new ChartSeries("CPU %", points, "#E74C3C") },
            XAxisLabel: "снимок",
            YAxisLabel: "%",
            YAxisMin: 0,          // ← фиксируем
            YAxisMax: 100);       // ← фиксируем
    }

    public static ChartRequest MemoryTimeline(IReadOnlyList<ResourceSnapshot> snapshots)
    {
        var points = snapshots
            .Select((s, i) => new ChartPoint(i, s.Memory.UtilizationPercent))
            .ToArray();

        return new ChartRequest(
            Kind: ChartKind.Line,
            Title: "RAM за сутки",
            Series: new[] { new ChartSeries("RAM %", points, "#3498DB") },
            XAxisLabel: "снимок",
            YAxisLabel: "%",
            YAxisMin: 0,          // ← фиксируем
            YAxisMax: 100);       // ← фиксируем
    }

    public static ChartRequest GpuTimeline(IReadOnlyList<ResourceSnapshot> snapshots)
    {
        // Сохраняем ОРИГИНАЛЬНЫЙ индекс снимка (i), а не индекс после Where.
        // Это позволяет визуально совмещать CPU/RAM/GPU графики: точка №42
        // означает один и тот же момент времени на всех трёх графиках.
        var points = snapshots
            .Select((s, i) => (Index: i, Gpu: s.Gpu))
            .Where(x => x.Gpu is not null)
            .Select(x => new ChartPoint(x.Index, x.Gpu!.UtilizationPercent))
            .ToArray();

        return new ChartRequest(
            Kind: ChartKind.Line,
            Title: "GPU за сутки",
            Series: new[] { new ChartSeries("GPU %", points, "#27AE60") },
            XAxisLabel: "снимок",
            YAxisLabel: "%",
            YAxisMin: 0,
            YAxisMax: 100);
    }

    public static ChartRequest AnomaliesBySeverity(DailyReport report)
    {
        // Bar chart — здесь ось Y = «количество», авто-масштаб разумнее фиксации.
        var series = new ChartSeries("Аномалии", new[]
        {
            new ChartPoint((double)SeverityLevel.Info,     report.Anomalies.Count(a => a.Severity == SeverityLevel.Info)),
            new ChartPoint((double)SeverityLevel.Warning,  report.WarningAnomalyCount),
            new ChartPoint((double)SeverityLevel.Critical, report.CriticalAnomalyCount),
        });

        return new ChartRequest(ChartKind.Bar, "Аномалии по severity",
            new[] { series });
    }
}