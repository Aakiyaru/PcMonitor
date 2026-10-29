using PcMonitor.Application.Abstractions;
using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection.Metadata;

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

    public async Task ExportAsync(DailyReport report, CancellationToken ct = default)
    {
        // Лицензия — Community (бесплатно для проектов до $1M дохода)
        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.Header().Text($"Отчёт за {report.Date:yyyy-MM-dd}")
                    .FontSize(20).Bold();

                page.Content().Column(col =>
                {
                    col.Item().Text(
                        $"Снимков проанализировано: {report.SnapshotsAnalyzed}");
                    col.Item().Text(
                        $"Критичных аномалий: {report.CriticalAnomalyCount}");
                    col.Item().Text(
                        $"Предупреждений: {report.WarningAnomalyCount}");

                    col.Item().PaddingTop(15).Text("Топ-потребители:")
                        .FontSize(14).Bold();
                    foreach (var top in report.TopConsumers)
                    {
                        col.Item().Text(
                            $"{top.Process} — ср. {top.AverageValue:F1}, " +
                            $"пик {top.PeakValue:F1} ({top.Metric})");
                    }

                    col.Item().PaddingTop(15).Text("Аномалии:")
                        .FontSize(14).Bold();
                    foreach (var a in report.Anomalies.Take(20))
                    {
                        col.Item().Text(
                            $"{a.OccurredAt:HH:mm:ss} [{a.Severity}] " +
                            $"{a.Metric}: {a.Reason}");
                    }
                });
            });
        }).GeneratePdf();

        var path = Path.Combine(_outputDirectory,
            $"report-{report.Date:yyyy-MM-dd}.pdf");
        Directory.CreateDirectory(_outputDirectory);
        await File.WriteAllBytesAsync(path, pdfBytes, ct);
    }
}