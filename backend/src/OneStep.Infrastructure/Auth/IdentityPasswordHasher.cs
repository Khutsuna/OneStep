using Microsoft.AspNetCore.Identity;
using OneStep.Application.Abstractions;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Auth;

/// <summary>
/// Wraps ASP.NET Core Identity's PasswordHasher (PBKDF2, random salt per password).
/// We use Microsoft's tested implementation instead of writing our own hashing,
/// because home-made password hashing is a classic security mistake.
/// </summary>
public class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string passwordHash, string password)
    {
        var result = _hasher.VerifyHashedPassword(null!, passwordHash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
