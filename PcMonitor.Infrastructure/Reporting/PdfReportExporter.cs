using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.Entities;
using PcMonitor.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;

namespace PcMonitor.Infrastructure.Reporting;

public sealed class PdfReportExporter : IReportExporter
{
    private readonly IChartRenderer _charts;
    private readonly string _outputDirectory;

    public PdfReportExporter(IChartRenderer charts, string outputDirectory)
    {
        _charts = charts;
        _outputDirectory = outputDirectory;
    }

    public async Task ExportAsync(
    DailyReport report,
    IReadOnlyList<ResourceSnapshot> snapshots,
    CancellationToken ct = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // Pre-render графиков — до создания PDF-документа.
        byte[]? cpuChart = snapshots.Count > 0
            ? _charts.Render(DailyReportChartFactory.CpuTimeline(snapshots))
            : null;
        byte[]? memChart = snapshots.Count > 0
            ? _charts.Render(DailyReportChartFactory.MemoryTimeline(snapshots))
            : null;

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Header()
                    .Text($"Отчёт за {report.Date:yyyy-MM-dd}")
                    .FontSize(20).Bold();

                page.Content().Column(col =>
                {
                    // Счётчики
                    col.Item().Text($"Снимков проанализировано: {report.SnapshotsAnalyzed}");
                    col.Item().Text($"Критичных аномалий: {report.CriticalAnomalyCount}");
                    col.Item().Text($"Предупреждений: {report.WarningAnomalyCount}");

                    // Графики
                    if (cpuChart is not null)
                    {
                        col.Item().PaddingTop(15).Text("CPU за сутки:")
                            .FontSize(14).Bold();
                        col.Item().Image(cpuChart).FitWidth();
                    }

                    if (memChart is not null)
                    {
                        col.Item().PaddingTop(10).Text("RAM за сутки:")
                            .FontSize(14).Bold();
                        col.Item().Image(memChart).FitWidth();
                    }

                    // Топ-потребители
                    col.Item().PaddingTop(15).Text("Топ-потребители:")
                        .FontSize(14).Bold();

                    foreach (var metric in new[] { MetricType.Cpu, MetricType.Memory, MetricType.DiskIo })
                    {
                        var tops = report.GetTopConsumers(metric);
                        if (tops.Count == 0) continue;

                        col.Item().PaddingTop(8).Text($"По {MetricLabel(metric)}:")
                            .FontSize(12).Bold();

                        foreach (var top in tops)
                        {
                            col.Item().Text(
                                $"  {top.Process} — ср. {FormatValue(top.Metric, top.AverageValue)}, " +
                                $"пик {FormatValue(top.Metric, top.PeakValue)}");
                        }
                    }

                    // Аномалии
                    col.Item().PaddingTop(15).Text("Аномалии:")
                        .FontSize(14).Bold();
                    if (report.Anomalies.Count == 0)
                    {
                        col.Item().Text("Аномалий не обнаружено.").Italic();
                    }
                    else
                    {
                        foreach (var a in report.Anomalies.Take(20))
                        {
                            col.Item().Text(
                                $"{a.OccurredAt:HH:mm:ss} [{SeverityLabel(a.Severity)}] " +
                                $"{MetricLabel(a.Metric)}: {a.Reason}");
                        }
                    }
                });
            });
        }).GeneratePdf();

        var path = Path.Combine(_outputDirectory, $"report-{report.Date:yyyy-MM-dd}.pdf");
        Directory.CreateDirectory(_outputDirectory);
        await File.WriteAllBytesAsync(path, pdfBytes, ct);
        Console.WriteLine($"PDF written: {path} ({pdfBytes.Length:N0} bytes)");
    }

    // В PdfReportExporter.cs
    private static string MetricLabel(MetricType m) => m switch
    {
        MetricType.Cpu => "CPU",
        MetricType.Memory => "RAM",
        MetricType.DiskIo => "Диск",
        MetricType.Gpu => "GPU",
        _ => m.ToString()
    };

    private static string SeverityLabel(SeverityLevel s) => s switch
    {
        SeverityLevel.Info => "Инфо",
        SeverityLevel.Warning => "Внимание",
        SeverityLevel.Critical => "Критично",
        _ => s.ToString()
    };

    private static string FormatValue(MetricType metric, double value) => metric switch
    {
        MetricType.Cpu => $"{value:F1}%",
        MetricType.Memory => FormatBytes((long)value),
        MetricType.Gpu => $"{value:F1}%",
        MetricType.DiskIo => FormatBytes((long)value) + "/с",
        _ => $"{value:F1}"
    };

    private static string FormatBytes(long bytes)
    {
        const long KB = 1024L, MB = KB * 1024, GB = MB * 1024;
        return bytes switch
        {
            >= GB => $"{bytes / (double)GB:F1} ГБ",
            >= MB => $"{bytes / (double)MB:F1} МБ",
            >= KB => $"{bytes / (double)KB:F1} КБ",
            _ => $"{bytes} Б"
        };
    }
}