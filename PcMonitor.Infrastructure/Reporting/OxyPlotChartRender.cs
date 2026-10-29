using OxyPlot;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;
using PcMonitor.Application.Abstractions;
using PcMonitor.Application.DTOs;

namespace PcMonitor.Infrastructure.Reporting;

public sealed class OxyPlotChartRenderer : IChartRenderer
{
    public byte[] Render(ChartRequest request)
    {
        var model = new PlotModel { Title = request.Title };
        if (request.XAxisLabel != null) model.Axes.Add(new OxyPlot.Axes.LinearAxis
        { Position = OxyPlot.Axes.AxisPosition.Bottom, Title = request.XAxisLabel });
        if (request.YAxisLabel != null) model.Axes.Add(new OxyPlot.Axes.LinearAxis
        { Position = OxyPlot.Axes.AxisPosition.Left, Title = request.YAxisLabel });

        foreach (var series in request.Series)
        {
            switch (request.Kind)
            {
                case ChartKind.Line:
                    var line = new LineSeries { Title = series.Name };
                    foreach (var pt in series.Points)
                        line.Points.Add(new DataPoint(pt.X, pt.Y));
                    model.Series.Add(line);
                    break;
                case ChartKind.Bar:
                    var bar = new BarSeries { Title = series.Name };
                    foreach (var pt in series.Points)
                        bar.Items.Add(new BarItem(pt.Y));
                    model.Series.Add(bar);
                    break;
            }
        }

        using var stream = new MemoryStream();
        var exporter = new PngExporter { Width = 1200, Height = 600 };
        exporter.Export(model, stream);
        return stream.ToArray();
    }
}