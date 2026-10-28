using PcMonitor.Domain.Common;

namespace PcMonitor.Domain.ValueObjects;

public readonly record struct ProcessIdentifier
{
    public int Pid { get; }
    public string Name { get; }

    public ProcessIdentifier(int pid, string name)
    {
        Guard.AgainstNegativeOrZero(pid, nameof(pid));
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Pid = pid;
        Name = name;
    }

    public override string ToString() => $"{Name} ({Pid})";
}