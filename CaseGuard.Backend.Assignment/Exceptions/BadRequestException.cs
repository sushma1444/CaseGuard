namespace CaseGuard.Backend.Assignment.Exceptions;

/// <summary>
/// Exception thrown when a request is invalid or malformed.
/// Returns 400 Bad Request status code.
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
