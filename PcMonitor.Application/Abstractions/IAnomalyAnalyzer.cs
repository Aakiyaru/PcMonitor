using PcMonitor.Application.DTOs;
using PcMonitor.Domain.Entities;

namespace PcMonitor.Application.Abstractions
{
    public interface IAnomalyAnalyzer
    {
        string Name { get; }
        Task<IReadOnlyList<Anomaly>> AnalyzeAsync(AnalysisContext context, CancellationToken ct = default);
    }
}
