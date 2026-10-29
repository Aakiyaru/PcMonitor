namespace PcMonitor.Application.DTOs
{
    public enum ChartKind { Line, Bar, Pie }

    public sealed record ChartRequest(
        ChartKind Kind,
        string Title,
        IReadOnlyList<ChartSeries> Series,
        string? YAxisLabel = null,
        string? XAxisLabel = null);
        

    public sealed record ChartSeries(
        string Name,
        IReadOnlyList<ChartPoint> Points,
        string? Color = null);

    public sealed record ChartPoint(double x, double y);
}
