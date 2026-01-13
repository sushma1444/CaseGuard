namespace CaseGuard.Backend.Assignment.Exceptions;

/// <summary>
/// Exception thrown when the user is authenticated but lacks permission to perform the action.
/// Returns 403 Forbidden status code.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }

    public ForbiddenException() : base("You do not have permission to perform this action.")
    {
    }
}
