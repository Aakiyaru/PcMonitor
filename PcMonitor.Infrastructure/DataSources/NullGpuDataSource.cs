using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Infrastructure.DataSources;

/// <summary>
/// Fallback: используется, если LibreHardwareMonitor не может
/// инициализироваться (нет прав, нет GPU, неподдерживаемая ОС).
/// </summary>
public sealed class NullGpuDataSource : IDataSource<GpuMetrics?>
{
    public string Name => "gpu-null";
    public GpuMetrics? Read() => null;
}