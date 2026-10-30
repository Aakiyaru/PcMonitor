// PcMonitor.Infrastructure/Gpu/IntelIgclProvider.cs
using PcMonitor.Domain.ValueObjects;
using System.Runtime.InteropServices;

namespace PcMonitor.Infrastructure.GPU;

public sealed class IntelIgclProvider : IGpuTelemetryProvider
{
    public string VendorName => "Intel (IGCL)";

    public bool TryInitialize()
    {
        try
        {
            // Проверяем, что DLL доступна
            return NativeMethods.ctlInit() == 0;
        }
        catch (DllNotFoundException) { return false; }
    }

    public GpuMetrics? ReadTelemetry()
    {
        try
        {
            // Упрощённо: в реальности нужно получить handle устройства
            // и вызвать ctlGetDeviceProperties / ctlGetTemperature и т.д.
            // Сигнатуры зависят от версии igcl_api.h.
            return null; // заглушка, пока не реализован полный P/Invoke
        }
        catch { return null; }
    }

    public void Dispose() { /* ctlClose() */ }

    private static class NativeMethods
    {
        [DllImport("ControlLib.dll")]
        public static extern int ctlInit();
    }
}