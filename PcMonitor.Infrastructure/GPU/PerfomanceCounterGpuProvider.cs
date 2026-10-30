// PcMonitor.Infrastructure/Gpu/PerformanceCounterGpuProvider.cs
using PcMonitor.Domain.ValueObjects;
using System.Diagnostics;

namespace PcMonitor.Infrastructure.GPU;

public sealed class PerformanceCounterGpuProvider : IGpuTelemetryProvider
{
    // Кэш счётчиков: instanceName → PerformanceCounter.
    // Первый NextValue() при создании прогревает baseline,
    // последующие вызовы возвращают реальные значения.
    private readonly Dictionary<string, PerformanceCounter> _counters = new();
    private readonly object _gate = new();
    private bool _initialized;

    public string VendorName => "Windows Performance Counters";

    public bool TryInitialize()
    {
        _initialized = true;
        return true;
    }

    public GpuMetrics? ReadTelemetry()
    {
        if (!_initialized) return null;

        try
        {
            var category = new PerformanceCounterCategory("GPU Engine");
            var instances = category.GetInstanceNames();
            if (instances.Length == 0) return null;

            double total;

            lock (_gate)
            {
                // 1. Удаляем счётчики для исчезнувших инстансов
                //    (окна/вкладки закрылись).
                var stale = _counters.Keys.Except(instances).ToList();
                foreach (var key in stale)
                {
                    _counters[key].Dispose();
                    _counters.Remove(key);
                }

                // 2. Создаём счётчики для новых инстансов
                //    (открылись новые окна/вкладки).
                foreach (var instance in instances)
                {
                    if (_counters.ContainsKey(instance)) continue;

                    var counter = new PerformanceCounter(
                        "GPU Engine", "Utilization Percentage", instance);
                    counter.NextValue();   // прогрев baseline — вернёт 0
                    _counters[instance] = counter;
                }

                // 3. Считываем текущие значения со всех счётчиков.
                total = 0;
                foreach (var counter in _counters.Values)
                {
                    total += counter.NextValue();
                }
            }

            return new GpuMetrics(
                deviceName: "GPU (Windows counters)",
                utilizationPercent: Math.Clamp(total, 0, 100),
                temperatureCelsius: null,
                memoryUsedBytes: null,
                memoryTotalBytes: null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GPU] PerformanceCounter failed: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var c in _counters.Values) c.Dispose();
            _counters.Clear();
        }
    }
}