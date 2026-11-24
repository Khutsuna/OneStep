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
    private readonly IEmailSender _emailSender;

    public AuthService(
        IAppDbContext db, IPasswordHasher passwordHasher, ITokenService tokenService, IEmailSender emailSender)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailSender = emailSender;
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

    // ---------- Password reset ----------

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);

        // Unknown email: do nothing, but the controller answers exactly the same way (204).
        // Otherwise this form would tell an attacker which emails have accounts.
        if (user is null)
        {
            return;
        }

        var token = PasswordResetRules.GenerateToken();

        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = PasswordResetRules.HashToken(token),       // store the hash, never the token
            ExpiresAt = DateTime.UtcNow.Add(PasswordResetRules.Lifetime)
        });
        await _db.SaveChangesAsync(cancellationToken);

        // The real token leaves the server only inside the email (in development: the log).
        await _emailSender.SendPasswordResetAsync(user.Email, user.FirstName, token, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        // 1. The database stores only hashes, so hash the token from the link the same way.
        var tokenHash = PasswordResetRules.HashToken(request.Token);

        // 2. Find that token, and load its user with it (we need to change the user's password).
        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        // 3. Unknown, expired or already used token -> 400. One message for all three cases.
        //    "resetToken is null" must come first: if it is null, resetToken.User would crash.
        var now = DateTime.UtcNow;
        if (resetToken is null || !resetToken.User.IsActive || !PasswordResetRules.IsUsable(resetToken, now))
        {
            throw new BadRequestException(PasswordResetRules.InvalidTokenMessage);
        }

        // 4. Store the HASH of the new password (PBKDF2), never the password itself.
        resetToken.User.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        // 5. Mark the token as used, so the same link cannot be used a second time.
        resetToken.UsedAt = now;

        // 6. One SaveChanges = one transaction: the password and "used" are saved together, or neither.
        await _db.SaveChangesAsync(cancellationToken);
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