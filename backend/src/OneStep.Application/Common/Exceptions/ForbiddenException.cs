namespace OneStep.Application.Common.Exceptions;

/// <summary>
/// Throw this when the user is logged in but not allowed to do this.
/// The global exception handler turns it into a 403 Forbidden response.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
