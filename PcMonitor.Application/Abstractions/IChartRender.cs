using PcMonitor.Application.DTOs;

namespace PcMonitor.Application.Abstractions
{
    public interface IChartRender
    {
        byte[] Render(ChartRequest request);
    }
}
