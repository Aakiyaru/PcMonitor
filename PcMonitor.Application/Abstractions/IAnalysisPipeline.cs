using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;

namespace PcMonitor.Application.Abstractions
{
    public interface IAnalysisPipeline
    {
        Task<IReadOnlyList<Anomaly>> RunAsync(AnalysisContext context, CancellationToken ct = default);
    }
}
