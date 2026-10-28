using System.Diagnostics;
using PcMonitor.Application.Abstractions;
using PcMonitor.Domain.ValueObjects;

namespace PcMonitor.Infrastructure.DataSources;

public sealed class PerformanceCounterCpuDataSource
    : IDataSource<CpuUsage>, IDisposable
{
    private readonly PerformanceCounter _counter;

    public PerformanceCounterCpuDataSource()
    {
        _counter = new PerformanceCounter(
            categoryName: "Processor",
            counterName: "% Processor Time",
            instanceName: "_Total",
            readOnly: true);

        // Прогрев: первый NextValue() вернёт 0, второй — реальное значение
        // через ~1 секунду. Делаем «холостое» чтение прямо в конструкторе,
        // чтобы первый Read() из оркестратора дал валидное число.
        _ = _counter.NextValue();
    }

    public string Name => "cpu-performance-counter";

    public CpuUsage Read()
    {
        var value = _counter.NextValue();

        // Иногда счётчик возвращает значения чуть выше 100 из-за гонки;
        // CpuUsage валидирует [0,100], поэтому подрезаем.
        var clamped = Math.Clamp(value, 0f, 100f);
        return new CpuUsage(clamped);
    }

    public void Dispose() => _counter.Dispose();
}