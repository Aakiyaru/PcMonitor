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
            YAxisLabel: "%");
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
            YAxisLabel: "%");
    }

    public static ChartRequest AnomaliesBySeverity(DailyReport report)
    {
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