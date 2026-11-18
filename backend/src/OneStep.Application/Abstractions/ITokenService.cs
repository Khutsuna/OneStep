using OneStep.Domain.Entities;

namespace OneStep.Application.Abstractions;

/// <summary>Creates the signed JWT access token a user receives after logging in.</summary>
public interface ITokenService
{
    TokenResult CreateToken(User user);
}

public record TokenResult(string AccessToken, DateTime ExpiresAt);
