namespace DotnetApi.Domain.Exceptions;

/// <summary>
/// Thrown when a domain business rule is violated
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
