namespace PcMonitor.Domain.Common;

/// <summary>
/// Нарушение бизнес-инварианта домена.
/// Все исключения домена наследуются от этого типа, чтобы
/// Application-слой мог отличать их от инфраструктурных ошибок.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}