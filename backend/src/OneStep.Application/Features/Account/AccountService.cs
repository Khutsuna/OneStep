using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Entities;

namespace OneStep.Application.Features.Account;

/// <summary>The logged-in user's own profile and password. The user id always comes from the token.</summary>
public class AccountService : IAccountService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    public AccountService(IAppDbContext db, ICurrentUser currentUser, IPasswordHasher passwordHasher)
    {
        _db = db;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    private int UserId => _currentUser.UserId ?? throw new UnauthorizedException("Please log in.");

    public async Task<ProfileDto> GetProfileAsync(CancellationToken cancellationToken)
    {
        return ToDto(await LoadUserAsync(cancellationToken));
    }

    public async Task<ProfileDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await LoadUserAsync(cancellationToken);
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(user);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await LoadUserAsync(cancellationToken);

        // Asking for the current password stops someone who finds an unlocked, logged-in computer.
        if (!_passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
        {
            throw new BadRequestException("Your current password is not correct.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> LoadUserAsync(CancellationToken cancellationToken)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == UserId, cancellationToken)
            ?? throw new NotFoundException("Your account was not found.");
    }

    private static ProfileDto ToDto(User u) => new(u.Id, u.Email, u.FirstName, u.LastName, u.PhoneNumber, u.CreatedAt);
}
