namespace DotnetApi.Application.Common.Exceptions;

/// <summary>
/// Thrown when input validation fails at the application layer
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>
        {
            { "General", [message] }
        };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
