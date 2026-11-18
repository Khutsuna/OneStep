namespace OneStep.Application.Common.Exceptions;

/// <summary>
/// Throw this when the request clashes with the current data, e.g. an email that is already registered.
/// The global exception handler turns it into a 409 Conflict response.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
