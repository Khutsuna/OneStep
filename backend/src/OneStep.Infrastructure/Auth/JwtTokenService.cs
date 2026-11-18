using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OneStep.Application.Abstractions;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Auth;

/// <summary>
/// Creates a JWT: a signed piece of text containing "claims" about the user
/// (id, email, name, role) and an expiry time. The API signs it with a secret key,
/// so if anyone changes a claim (e.g. role = Admin), the signature no longer matches
/// and the API rejects the token.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public TokenResult CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),     // "sub" = subject = user id
                [JwtRegisteredClaimNames.Email] = user.Email,
                ["name"] = $"{user.FirstName} {user.LastName}",
                ["role"] = user.Role.ToString()
            }
        };

        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenResult(accessToken, expiresAt);
    }
}
