// PcMonitor.Infrastructure/Gpu/NvmlNative.cs
using System;
using System.Runtime.InteropServices;

namespace PcMonitor.Infrastructure.GPU;

internal static class NvmlNative
{
    private const string NvmlDll = "nvml.dll";

    public const int NVML_SUCCESS = 0;
    public const int NVML_TEMPERATURE_GPU = 0;

    [StructLayout(LayoutKind.Sequential)]
    public struct NvmlUtilization { public uint Gpu; public uint Memory; }

    [StructLayout(LayoutKind.Sequential)]
    public struct NvmlMemory { public ulong Total; public ulong Free; public ulong Used; }

    [DllImport(NvmlDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int nvmlInit_v2();

    [DllImport(NvmlDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int nvmlShutdown();

    [DllImport(NvmlDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int nvmlDeviceGetHandleByIndex_v2(uint index, out IntPtr device);

    [DllImport(NvmlDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int nvmlDeviceGetUtilizationRates(IntPtr device, out NvmlUtilization util);

    [DllImport(NvmlDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int nvmlDeviceGetMemoryInfo(IntPtr device, out NvmlMemory mem);

    [DllImport(NvmlDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int nvmlDeviceGetTemperature(IntPtr device, int sensorType, out uint temp);

    [DllImport(NvmlDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int nvmlDeviceGetName(IntPtr device, System.Text.StringBuilder name, uint length);
}