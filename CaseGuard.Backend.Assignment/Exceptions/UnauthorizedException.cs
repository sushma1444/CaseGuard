namespace CaseGuard.Backend.Assignment.Exceptions;

/// <summary>
/// Exception thrown when authentication is required or failed.
/// Returns 401 Unauthorized status code.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException() : base("Authentication required.")
    {
    }
}
