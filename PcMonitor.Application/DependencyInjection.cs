using Microsoft.Extensions.DependencyInjection;
using PcMonitor.Application.Abstractions;
using PcMonitor.Application.Analyzers;
using PcMonitor.Application.Services;
using PcMonitor.Application.UseCases;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        MetricThresholds? thresholds = null,
        TimeSpan? samplingInterval = null)
    {
        // Конфигурация домена — singleton, class, OK.
        services.AddSingleton(thresholds ?? new MetricThresholds());

        // Вместо голого TimeSpan — обёртка в record-класс.
        services.AddSingleton(new SamplingOptions(
            samplingInterval ?? TimeSpan.FromSeconds(10)));

        // Оркестратор — DI разрешит все зависимости.
        services.AddScoped<ICollectorOrchestrator, CollectorOrchestrator>();

        // Анализаторы: конструкторы принимают TimeSpan напрямую —
        // поэтому регистрируем через factory, а не типа.
        services.AddScoped<IAnomalyAnalyzer>(sp =>
            new CpuSpikeAnalyzer(
                sp.GetRequiredService<MetricThresholds>(),
                sp.GetRequiredService<SamplingOptions>().Interval));

        services.AddScoped<IAnomalyAnalyzer>(sp =>
            new MemoryPressureAnalyzer(
                sp.GetRequiredService<MetricThresholds>(),
                sp.GetRequiredService<SamplingOptions>().Interval));

        services.AddScoped<IAnomalyAnalyzer>(sp =>
            new DiskBurstAnalyzer(
                sp.GetRequiredService<MetricThresholds>(),
                sp.GetRequiredService<SamplingOptions>().Interval));

        services.AddScoped<IAnomalyAnalyzer>(sp =>
            new GpuSpikeAnalyzer(
                sp.GetRequiredService<MetricThresholds>(),
                sp.GetRequiredService<SamplingOptions>().Interval));

        // Enrichers
        services.AddScoped<IAnomalyEnricher, EventCorrelationEnricher>();

        // Pipeline
        services.AddScoped<IAnalysisPipeline, AnalysisPipeline>();

        // Report
        services.AddScoped<IReportBuilder, DailyReportBuilder>();

        // Use Cases
        services.AddScoped<CollectSnapshotUseCase>();
        services.AddScoped<GenerateDailyReportUseCase>();

        return services;
    }
}