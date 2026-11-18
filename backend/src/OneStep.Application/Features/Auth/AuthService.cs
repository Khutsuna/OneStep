using Microsoft.EntityFrameworkCore;

using OneStep.Application.Abstractions;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Entities;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IAppDbContext db, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        var emailTaken = await _db.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (emailTaken)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),   // never store the real password
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.Customer                                   // sign-ups are always customers
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        // Same message for "no such email" and "wrong password",
        // so an attacker can't use the login form to find out which emails are registered.
        if (user is null || !user.IsActive || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<UserDto> GetUserAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user is null
            ? throw new NotFoundException("User not found.")
            : ToDto(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var token = _tokenService.CreateToken(user);
        return new AuthResponse(token.AccessToken, token.ExpiresAt, ToDto(user));
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString());
    }

    // "Nino@Example.com " and "nino@example.com" must be the same account.
    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}