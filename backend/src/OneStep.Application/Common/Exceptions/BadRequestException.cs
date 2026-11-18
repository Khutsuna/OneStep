namespace OneStep.Application.Common.Exceptions;

/// <summary>
/// Throw this when the request breaks a business rule.
/// The global exception handler turns it into a 400 Bad Request response.
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}
