using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

/// <summary>
/// Stores only a SHA-256 hash of the reset token, never the token itself,
/// so a database leak cannot be used to reset anyone's password.
/// </summary>
public class PasswordResetToken : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
