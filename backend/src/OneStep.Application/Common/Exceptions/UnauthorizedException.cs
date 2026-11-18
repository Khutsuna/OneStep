namespace OneStep.Application.Common.Exceptions;

/// <summary>
/// Throw this when login fails (wrong email or password, or a deactivated account).
/// The global exception handler turns it into a 401 Unauthorized response.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
