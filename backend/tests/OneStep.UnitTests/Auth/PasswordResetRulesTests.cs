using OneStep.Application.Features.Auth;
using OneStep.Domain.Entities;

namespace OneStep.UnitTests.Auth;

public class PasswordResetRulesTests
{
    private static readonly DateTime Now = new(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Generated_tokens_are_long_url_safe_and_never_repeat()
    {
        var first = PasswordResetRules.GenerateToken();
        var second = PasswordResetRules.GenerateToken();

        Assert.NotEqual(first, second);
        Assert.Equal(43, first.Length);                        // 32 random bytes as Base64 without '='
        Assert.DoesNotContain('+', first);
        Assert.DoesNotContain('/', first);
        Assert.DoesNotContain('=', first);
    }

    [Fact]
    public void The_same_token_always_gives_the_same_64_character_hash()
    {
        var token = PasswordResetRules.GenerateToken();

        var hash = PasswordResetRules.HashToken(token);

        Assert.Equal(64, hash.Length);                         // fits the CHAR(64) TokenHash column
        Assert.Equal(hash, PasswordResetRules.HashToken(token));
        Assert.NotEqual(token, hash);                          // the stored value is not the token itself
    }

    [Fact]
    public void A_fresh_unused_token_is_usable()
    {
        var token = new PasswordResetToken { ExpiresAt = Now.AddMinutes(30) };

        Assert.True(PasswordResetRules.IsUsable(token, Now));
    }

    [Fact]
    public void An_expired_token_is_not_usable()
    {
        var token = new PasswordResetToken { ExpiresAt = Now.AddMinutes(-1) };

        Assert.False(PasswordResetRules.IsUsable(token, Now));
    }

    [Fact]
    public void A_token_works_only_once()
    {
        var token = new PasswordResetToken { ExpiresAt = Now.AddMinutes(30), UsedAt = Now.AddMinutes(-5) };

        Assert.False(PasswordResetRules.IsUsable(token, Now));
    }
}
