// PcMonitor.Infrastructure/Gpu/GpuVendorDetector.cs
using System.Management;

namespace PcMonitor.Infrastructure.GPU;

public static class GpuVendorDetector
{
    public static GpuVendor Detect()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT PNPDeviceID, Name FROM Win32_VideoController");

            foreach (ManagementObject obj in searcher.Get())
            {
                var pnpId = obj["PNPDeviceID"]?.ToString();
                var name = obj["Name"]?.ToString();
                if (string.IsNullOrEmpty(pnpId)) continue;

                if (pnpId.Contains("VEN_10DE")) return GpuVendor.Nvidia;
                if (pnpId.Contains("VEN_1002")) return GpuVendor.Amd;
                if (pnpId.Contains("VEN_8086")) return GpuVendor.Intel;

                Console.WriteLine($"[GPU] Unknown vendor: {name} ({pnpId})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GPU] WMI detection failed: {ex.Message}");
        }

        return GpuVendor.Unknown;
    }
}