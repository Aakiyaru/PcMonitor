// PcMonitor.Infrastructure/Gpu/NvidiaNvmlProvider.cs
using System.Text;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Infrastructure.GPU;

public sealed class NvidiaNvmlProvider : IGpuTelemetryProvider
{
    private IntPtr _device = IntPtr.Zero;
    private string _name = "NVIDIA GPU";
    private bool _initialized;

    public string VendorName => "NVIDIA (NVML)";

    public bool TryInitialize()
    {
        try
        {
            if (NvmlNative.nvmlInit_v2() != NvmlNative.NVML_SUCCESS) return false;

            if (NvmlNative.nvmlDeviceGetHandleByIndex_v2(0, out _device)
                != NvmlNative.NVML_SUCCESS) return false;

            // Имя карты — для отчёта.
            var sb = new StringBuilder(96);
            if (NvmlNative.nvmlDeviceGetName(_device, sb, (uint)sb.Capacity)
                == NvmlNative.NVML_SUCCESS)
            {
                _name = sb.ToString();
            }

            _initialized = true;
            Console.WriteLine($"[GPU] NVML initialized: {_name}");
            return true;
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("[GPU] nvml.dll not found (no NVIDIA driver?)");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GPU] NVML init failed: {ex.Message}");
            return false;
        }
    }

    public GpuMetrics? ReadTelemetry()
    {
        if (!_initialized) return null;

        try
        {
            var utilRes = NvmlNative.nvmlDeviceGetUtilizationRates(_device, out var util);
            var memRes = NvmlNative.nvmlDeviceGetMemoryInfo(_device, out var mem);
            var tempRes = NvmlNative.nvmlDeviceGetTemperature(
                _device, NvmlNative.NVML_TEMPERATURE_GPU, out var temp);

            if (utilRes != NvmlNative.NVML_SUCCESS || memRes != NvmlNative.NVML_SUCCESS)
                return null;

            return new GpuMetrics(
                deviceName: _name,
                utilizationPercent: Math.Clamp(util.Gpu, 0, 100),
                temperatureCelsius: tempRes == NvmlNative.NVML_SUCCESS ? temp : null,
                memoryUsedBytes: (long)mem.Used,
                memoryTotalBytes: (long)mem.Total);
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (_initialized)
        {
            try { NvmlNative.nvmlShutdown(); } catch { }
            _initialized = false;
        }
    }
}