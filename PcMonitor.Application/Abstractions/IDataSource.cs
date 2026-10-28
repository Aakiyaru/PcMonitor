namespace PcMonitor.Application.Abstractions
{
    /// <summary>
    /// Абстракция одного источника метрик. Каждый конкретный
    /// источник (CPU, RAM, диск, GPU, процессы) реализует свой
    /// интерфейс с параметром <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Реализация может быть не потокобезопасной — Orchestrator
    /// вызывает <see cref="Read"/> последовательно.
    /// </remarks>
    public interface IDataSource<out T>
    {
        string Name { get; }
        T Read();
    }
}
