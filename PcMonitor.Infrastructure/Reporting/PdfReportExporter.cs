using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
        QuestPDF.Settings.License = LicenseType.Community;

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
                    col.Item().Text($"Снимков проанализировано: {report.SnapshotsAnalyzed}");
                    col.Item().Text($"Критичных аномалий: {report.CriticalAnomalyCount}");
                    col.Item().Text($"Предупреждений: {report.WarningAnomalyCount}");

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

        // Явная проверка: родительская папка существует и является папкой.
        EnsureDirectoryIsUsable(_outputDirectory);

        var path = Path.Combine(_outputDirectory, $"report-{report.Date:yyyy-MM-dd}.pdf");

        try
        {
            await File.WriteAllBytesAsync(path, pdfBytes, ct);
            Console.WriteLine($"PDF written: {path} ({pdfBytes.Length:N0} bytes)");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to write PDF to '{path}'. " +
                $"Directory exists: {Directory.Exists(_outputDirectory)}. " +
                $"Parent: '{Path.GetDirectoryName(_outputDirectory)}'. " +
                $"Check if 'PcMonitor' in Documents is a file instead of a directory.",
                ex);
        }
    }

    /// <summary>
    /// Проверяет, что путь существует, является папкой, и создаёт его при необходимости.
    /// Бросает осмысленное исключение, если родитель — файл или путь сломан.
    /// </summary>
    private static void EnsureDirectoryIsUsable(string path)
    {
        // Если путь уже существует, но это файл — CreateDirectory это молча «проглотит».
        if (File.Exists(path) && !Directory.Exists(path))
        {
            throw new InvalidOperationException(
                $"'{path}' exists as a FILE, not a directory. " +
                $"Delete it manually or use a different output directory.");
        }

        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Cannot create directory '{path}'. " +
                    $"Parent '{Path.GetDirectoryName(path)}' " +
                    $"(is file: {File.Exists(Path.GetDirectoryName(path))}).",
                    ex);
            }
        }

        // После CreateDirectory — проверяем реально.
        if (!Directory.Exists(path))
        {
            throw new InvalidOperationException(
                $"Directory '{path}' does not exist after CreateDirectory. " +
                $"Parent may be a reparse point or file. " +
                $"Check with: Get-Item '{Path.GetDirectoryName(path)}'.");
        }
    }
}