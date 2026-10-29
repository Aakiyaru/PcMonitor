using LibreHardwareMonitor.Hardware;
using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.ValueObjects;
namespace PcMonitor.Infrastructure.DataSources;

public sealed class LibreHardwareMonitorGpuDataSource
    : IDataSource<GpuMetrics?>, IDisposable
{
    private readonly Computer _computer;

    public LibreHardwareMonitorGpuDataSource()
    {
        _computer = new Computer
        {
            IsGpuEnabled = true,
            IsCpuEnabled = false,
            IsMemoryEnabled = false,
            IsMotherboardEnabled = false,
            IsStorageEnabled = false,
        };
        _computer.Open();
    }

    public string Name => "gpu-lhm";

    public GpuMetrics? Read()
    {
        foreach (var hw in _computer.Hardware)
        {
            if (!IsGpu(hw.HardwareType)) continue;

            hw.Update();
            var name = hw.Name;
            var util = FindSensor(hw, SensorType.Load, "GPU Core")?.Value ?? 0;
            var temp = FindSensor(hw, SensorType.Temperature, "GPU Core")?.Value;
            var memUsed = FindSensor(hw, SensorType.SmallData, "GPU Memory Used")?.Value;
            var memTotal = FindSensor(hw, SensorType.SmallData, "GPU Memory Total")?.Value;

            return new GpuMetrics(
                deviceName: name,
                utilizationPercent: Math.Clamp(util, 0, 100),
                temperatureCelsius: temp,
                memoryUsedBytes: memUsed is { } mu ? (long)(mu * 1024 * 1024) : null,
                memoryTotalBytes: memTotal is { } mt ? (long)(mt * 1024 * 1024) : null);
        }

        return null;  // GPU не обнаружен или не поддерживается
    }

    public void Dispose() => _computer.Close();

    private static bool IsGpu(HardwareType type) => type
        is HardwareType.GpuNvidia
        or HardwareType.GpuAmd
        or HardwareType.GpuIntel;

    private static ISensor? FindSensor(IHardware hw, SensorType type, string name)
        => hw.Sensors.FirstOrDefault(s =>
            s.SensorType == type
            && s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
}