using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PcMonitor.Application;
using PcMonitor.Application.Abstractions;
using PcMonitor.Application.UseCases;
using PcMonitor.Domain.ValueObjects;
using PcMonitor.Infrastructure;
using PcMonitor.Infrastructure.Events;
using PcMonitor.Infrastructure.Persistence;
using PcMonitor.Presentation.Worker.Logging;
using PcMonitor.Presentation.Worker.Options;
using PcMonitor.Presentation.Worker.Workers;

// -----------------------------------------------------------------------------
// 1. CLI-режим: ручная генерация отчёта за конкретную дату.
//    dotnet run -- generate-report 2024-01-15
//    Полезно для проверки экспорта, не дожидаясь ночи.
// -----------------------------------------------------------------------------
if (args.Length >= 2 && args[0] == "generate-report")
{
    if (!DateOnly.TryParse(args[1], out var date))
    {
        Console.Error.WriteLine($"Invalid date: {args[1]}. Expected format: yyyy-MM-dd");
        return 1;
    }
    return await RunManualReportAsync(date);
}

// -----------------------------------------------------------------------------
// 2. Основной режим: фоновые службы.
// -----------------------------------------------------------------------------
return await RunWorkerAsync(args);


// =============================================================================
// Реализация
// =============================================================================

static async Task<int> RunWorkerAsync(string[] args)
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.Configure<WorkerOptions>(
        builder.Configuration.GetSection("Worker"));

    // Валидация конфигурации при старте — падаем сразу, если пороги противоречат.
    builder.Services
        .AddOptions<WorkerOptions>()
        .Validate(o => o.SamplingIntervalSeconds >= 1,
            "SamplingIntervalSeconds must be >= 1")
        .Validate(o => o.CpuWarnPercent < o.CpuCriticalPercent,
            "CpuWarnPercent must be less than CpuCriticalPercent")
        .Validate(o => o.MemoryWarnPercent < o.MemoryCriticalPercent,
            "MemoryWarnPercent must be less than MemoryCriticalPercent")
        .Validate(o => o.DiskWarnMegabytesPerSecond < o.DiskCriticalMegabytesPerSecond,
            "DiskWarn must be less than DiskCritical")
        .ValidateOnStart();

    var workerOpts = builder.Configuration
        .GetSection("Worker")
        .Get<WorkerOptions>() ?? new WorkerOptions();

    // Пути к данным.
    var dbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PcMonitor", "monitor.db");
    Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

    // Отчёты: сначала пробуем Documents (удобно пользователю),
    // при любой проблеме — fallback в LocalApplicationData,
    // чтобы фоновый сервис не падал при старте.
    var reportsDir = ResolveReportsDirectory();
    Directory.CreateDirectory(reportsDir);

    // ... ниже — остальной код без изменений

    // =============================================================================
    // Хелпер: безопасное определение папки для отчётов
    // =============================================================================
    static string ResolveReportsDirectory()
    {
        // Попытка 1: Documents — самое удобное место для пользователя.
        try
        {
            var documents = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments);

            if (!string.IsNullOrWhiteSpace(documents))
            {
                var candidate = Path.Combine(documents, "PcMonitor", "Reports");

                // Directory.Exists на родителя проверяет реальность папки,
                // а не только намерение системы.
                var parent = Path.GetDirectoryName(candidate)!;
                if (Directory.Exists(parent) || !File.Exists(parent))
                {
                    Directory.CreateDirectory(candidate);
                    Console.WriteLine($"Reports directory: {candidate}");
                    return candidate;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Cannot use Documents folder for reports: {ex.Message}");
        }

        // Fallback: LocalApplicationData — всегда доступна процессу.
        var fallback = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PcMonitor", "Reports");

        Directory.CreateDirectory(fallback);
        Console.WriteLine($"Reports directory (fallback): {fallback}");
        return fallback;
    }

    // Доменные пороги из конфига → MetricThresholds.
    var thresholds = new MetricThresholds
    {
        CpuWarnPercent = workerOpts.CpuWarnPercent,
        CpuCriticalPercent = workerOpts.CpuCriticalPercent,
        MemoryWarnPercent = workerOpts.MemoryWarnPercent,
        MemoryCriticalPercent = workerOpts.MemoryCriticalPercent,
        DiskWarnBytesPerSecond = workerOpts.DiskWarnMegabytesPerSecond * 1024 * 1024,
        DiskCriticalBytesPerSecond = workerOpts.DiskCriticalMegabytesPerSecond * 1024 * 1024,
        GpuWarnPercent = workerOpts.GpuWarnPercent,
        GpuCriticalPercent = workerOpts.GpuCriticalPercent,
    };

    builder.Services
        .AddApplication(thresholds, TimeSpan.FromSeconds(workerOpts.SamplingIntervalSeconds))
        .AddInfrastructure(dbPath, reportsDir);

    // Подписчики на события.
    builder.Services.AddHostedService<DomainEventLoggingSubscriber>();

    // Фоновые службы.
    builder.Services.AddHostedService<MonitoringWorker>();
    builder.Services.AddHostedService<DailyReportWorker>();

    // Windows Service integration (работает и как консоль).
    builder.Services.AddWindowsService(o => o.ServiceName = "PcMonitor");

    var host = builder.Build();

    // Миграции — до старта hosted services.
    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<MonitorDbContext>();
        await db.Database.MigrateAsync();
    }

    await host.RunAsync();
    return 0;
}

static async Task<int> RunManualReportAsync(DateOnly date)
{
    var builder = Host.CreateApplicationBuilder(Array.Empty<string>());
    builder.Services.Configure<WorkerOptions>(
        builder.Configuration.GetSection("Worker"));

    var workerOpts = builder.Configuration
        .GetSection("Worker")
        .Get<WorkerOptions>() ?? new WorkerOptions();

    var dbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PcMonitor", "monitor.db");
    var reportsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "PcMonitor", "Reports");
    Directory.CreateDirectory(reportsDir);

    var thresholds = new MetricThresholds { /* как выше */ };
    builder.Services
        .AddApplication(thresholds, TimeSpan.FromSeconds(workerOpts.SamplingIntervalSeconds))
        .AddInfrastructure(dbPath, reportsDir);

    using var host = builder.Build();

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<MonitorDbContext>();
        await db.Database.MigrateAsync();

        var useCase = scope.ServiceProvider.GetRequiredService<GenerateDailyReportUseCase>();
        var report = await useCase.ExecuteAsync(date);

        Console.WriteLine(
            $"Report for {date} generated: " +
            $"{report.SnapshotsAnalyzed} snapshots, " +
            $"{report.CriticalAnomalyCount} critical, " +
            $"{report.WarningAnomalyCount} warnings, " +
            $"{report.Anomalies.Count} total anomalies.");

        var pdfPath = Path.Combine(reportsDir, $"report-{date:yyyy-MM-dd}.pdf");
        Console.WriteLine($"PDF: {pdfPath}");
    }

    return 0;
}