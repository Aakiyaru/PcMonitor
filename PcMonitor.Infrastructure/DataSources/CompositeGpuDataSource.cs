// PcMonitor.Infrastructure/DataSources/CompositeGpuDataSource.cs
using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.ValueObjects;
using PcMonitor.Infrastructure.GPU;

namespace PcMonitor.Infrastructure.DataSources;

public sealed class CompositeGpuDataSource : IDataSource<GpuMetrics?>, IDisposable
{
    private readonly IGpuTelemetryProvider _provider;

    public string Name => $"gpu ({_provider.VendorName})";

    public CompositeGpuDataSource()
    {
        var vendor = GpuVendorDetector.Detect();
        Console.WriteLine($"[GPU] Detected vendor: {vendor}");

        _provider = CreateProvider(vendor);

        if (!_provider.TryInitialize())
        {
            Console.WriteLine(
                $"[GPU] {_provider.VendorName} failed to initialize — using fallback.");

            _provider.Dispose();
            _provider = new PerformanceCounterGpuProvider();
            _provider.TryInitialize();
        }

        Console.WriteLine($"[GPU] Active provider: {_provider.VendorName}");
    }

    private static IGpuTelemetryProvider CreateProvider(GpuVendor vendor) => vendor switch
    {
        GpuVendor.Nvidia => new NvidiaNvmlProvider(),
        GpuVendor.Intel => new PerformanceCounterGpuProvider(),
        GpuVendor.Amd => new PerformanceCounterGpuProvider(),  // v1: без вендорского API
        _ => new PerformanceCounterGpuProvider()
    };

    public GpuMetrics? Read() => _provider.ReadTelemetry();
    public void Dispose() => _provider.Dispose();
}