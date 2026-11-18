namespace OneStep.Application.Common.Exceptions;

/// <summary>
/// Throw this when the requested item doesn't exist.
/// The global exception handler turns it into a 404 Not Found response.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
