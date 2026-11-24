using System.Security.Cryptography;
using System.Text;

using OneStep.Domain.Entities;

namespace OneStep.Application.Features.Auth;

/// <summary>
/// The rules for password reset tokens, as pure functions so they can be unit tested
/// (see PasswordResetRulesTests).
/// </summary>
public static class PasswordResetRules
{
    /// <summary>How long a reset link works.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

    public const string InvalidTokenMessage = "This reset link is invalid or has expired. Please request a new one.";

    /// <summary>
    /// A new random token: 32 bytes from a cryptographic random generator (not Random!),
    /// as URL-safe text so it can travel in a link: ?token=...
    /// </summary>
    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>
    /// The SHA-256 hash of a token, as 64 hex characters. Only this hash is stored,
    /// so someone who reads the database still cannot use the links.
    /// (SHA-256 is enough here because the token is long and random; passwords need PBKDF2.)
    /// </summary>
    public static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    /// <summary>A token works only once and only until it expires.</summary>
    public static bool IsUsable(PasswordResetToken token, DateTime now)
    {
        return token.UsedAt is null && token.ExpiresAt > now;
    }
}
