namespace PcMonitor.Application.DTOs;

public enum ChartKind { Line, Bar, Pie }

public sealed record ChartRequest(
    ChartKind Kind,
    string Title,
    IReadOnlyList<ChartSeries> Series,
    string? XAxisLabel = null,
    string? YAxisLabel = null,
    double? YAxisMin = null,      // ← новое
    double? YAxisMax = null);     // ← новое

public sealed record ChartSeries(
    string Name,
    IReadOnlyList<ChartPoint> Points,
    string? Color = null);

public sealed record ChartPoint(double X, double Y);