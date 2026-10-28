namespace PcMonitor.Domain.Common;

/// <summary>
/// Минимальный набор проверок, бросающих <see cref="DomainException"/>.
/// Специально не бросает ArgumentException — на уровне Domain
/// любое нарушение правила — это нарушение бизнес-инварианта.
/// </summary>
public static class Guard
{
    public static void AgainstNull(object? value, string paramName)
    {
        if (value is null)
            throw new DomainException($"{paramName} must not be null.");
    }

    public static void AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{paramName} must not be null or whitespace.");
    }

    public static void AgainstOutOfRange(double value, double min, double max, string paramName)
    {
        if (double.IsNaN(value) || value < min || value > max)
            throw new DomainException($"{paramName} must be in [{min}, {max}], but was {value}.");
    }

    public static void AgainstNegative(long value, string paramName)
    {
        if (value < 0)
            throw new DomainException($"{paramName} must be non-negative, but was {value}.");
    }

    public static void AgainstNegative(double value, string paramName)
    {
        if (double.IsNaN(value) || value < 0)
            throw new DomainException($"{paramName} must be non-negative, but was {value}.");
    }

    public static void AgainstNegative(int value, string paramName)
    {
        if (value < 0)
            throw new DomainException($"{paramName} must be non-negative, but was {value}.");
    }

    public static void AgainstNegativeOrZero(long value, string paramName)
    {
        if (value <= 0)
            throw new DomainException($"{paramName} must be positive, but was {value}.");
    }

    public static void AgainstNegativeOrZero(int value, string paramName)
    {
        if (value <= 0)
            throw new DomainException($"{paramName} must be positive, but was {value}.");
    }
}