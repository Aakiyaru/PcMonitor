using PcMonitor.Application.DTOs;

namespace PcMonitor.Application.Abstractions
{
    public interface IChartRenderer
    {
        byte[] Render(ChartRequest request);
    }
}
