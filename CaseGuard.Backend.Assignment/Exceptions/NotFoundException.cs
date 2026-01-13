namespace CaseGuard.Backend.Assignment.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// Returns 404 Not Found status code.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string resourceType, object resourceId) 
        : base($"{resourceType} with ID '{resourceId}' was not found.")
    {
    }
}
