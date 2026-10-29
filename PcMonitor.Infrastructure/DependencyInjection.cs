using LibreHardwareMonitor.Hardware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.Ports;
using PcMonitor.Domain.ValueObjects;
using PcMonitor.Infrastructure.Calendar;
using PcMonitor.Infrastructure.Clock;
using PcMonitor.Infrastructure.DataSources;
using PcMonitor.Infrastructure.Events;
using PcMonitor.Infrastructure.Persistence;
using PcMonitor.Infrastructure.Persistence.Repositories;
using PcMonitor.Infrastructure.Reporting;

namespace PcMonitor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string dbPath,
        string reportOutputDirectory)
    {
        // Cross-cutting
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IReportCalendar, LocalReportCalendar>();
        services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

        // Data sources — singleton, держат состояние между тиками
        services.AddSingleton<IDataSource<CpuUsage>, PerformanceCounterCpuDataSource>();
        services.AddSingleton<IDataSource<MemoryUsage>, PerformanceCounterMemoryDataSource>();
        services.AddSingleton<IDataSource<DiskIoRate>, PerformanceCounterDiskDataSource>();
        services.AddSingleton<IDataSource<IReadOnlyList<Domain.Entities.ProcessSnapshot>>,
            ProcessDataSource>();

        // GPU: пытаемся LHM, при провале (нет прав) — Null Object
        services.AddSingleton<IDataSource<GpuMetrics?>>(sp =>
        {
            try
            {
                var lhm = new LibreHardwareMonitorGpuDataSource();
                _ = lhm.Read();   // пробное чтение — упадёт, если нет драйвера
                return lhm;
            }
            catch
            {
                return new NullGpuDataSource();
            }
        });

        // Persistence
        services.AddDbContext<MonitorDbContext>(opt =>
            opt.UseSqlite($"Data Source={dbPath}"));
        services.AddScoped<IResourceSnapshotRepository, SqliteSnapshotRepository>();
        services.AddScoped<ISystemEventRepository, SqliteSystemEventRepository>();
        services.AddScoped<IDailyReportRepository, SqliteDailyReportRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Reporting
        services.AddSingleton<IChartRenderer, OxyPlotChartRenderer>();
        services.AddSingleton<IReportExporter>(sp =>
            new PdfReportExporter(
                sp.GetRequiredService<IChartRenderer>(),
                reportOutputDirectory));

        return services;
    }
}