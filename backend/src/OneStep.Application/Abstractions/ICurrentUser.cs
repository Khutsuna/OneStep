namespace OneStep.Application.Abstractions;

/// <summary>
/// Who is making the current request, read from the JWT token.
/// Services use this instead of trusting a user id sent in the request body.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    int? UserId { get; }
    string? Role { get; }
}
