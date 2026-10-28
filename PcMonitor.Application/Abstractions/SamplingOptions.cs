namespace PcMonitor.Application.Abstractions;

/// <summary>
/// DI-обёртка над интервалом сэмплирования. Нужна, чтобы
/// избежать primitive obsession: <see cref="TimeSpan"/> нельзя
/// зарегистрировать как singleton через generic-метод
/// (struct + constraint where T : class), и голый TimeSpan
/// в DI — это классическая ловушка «какой из двух?».
/// </summary>
public sealed record SamplingOptions
{
    public TimeSpan Interval { get; }

    public SamplingOptions(TimeSpan interval)
    {
        if (interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(interval), "Sampling interval must be positive.");
        Interval = interval;
    }
}