namespace OneStep.Infrastructure.Auth;

/// <summary>
/// JWT settings from the "Jwt" section of the configuration.
/// Issuer, Audience and ExpiresMinutes are in appsettings.json.
/// Key is a secret: it lives in user-secrets on your machine, never in Git.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 60;
}
