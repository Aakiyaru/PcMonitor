// PcMonitor.Infrastructure/Gpu/IGpuTelemetryProvider.cs
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Infrastructure.GPU;

public interface IGpuTelemetryProvider : IDisposable
{
    string VendorName { get; }
    bool TryInitialize();
    GpuMetrics? ReadTelemetry();
}

public enum GpuVendor { Unknown, Nvidia, Amd, Intel }