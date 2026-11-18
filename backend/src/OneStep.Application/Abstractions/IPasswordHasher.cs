namespace OneStep.Application.Abstractions;

/// <summary>
/// Turns a password into a one-way hash, and checks a password against a stored hash.
/// The real implementation (Infrastructure/Auth/IdentityPasswordHasher) uses
/// Microsoft's PasswordHasher: salted PBKDF2 with 100,000 iterations.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}
