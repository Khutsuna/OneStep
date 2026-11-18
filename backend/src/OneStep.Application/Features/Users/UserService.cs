using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Users;

/// <summary>User management for administrators.</summary>
public class UserService : IUserService
{
    private const int MaxPageSize = 100;

    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UserService(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<UserListItemDto>> GetUsersAsync(
        string? search, UserRole? role, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        // Build the query step by step; nothing runs in SQL until CountAsync / ToListAsync.
        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Email.Contains(term) || u.FirstName.Contains(term) || u.LastName.Contains(term));
        }

        if (role is not null)
        {
            query = query.Where(u => u.Role == role);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItemDto(
                u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.IsActive,
                u.CreatedAt, u.LastLoginAt, u.Orders.Count))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<UserListItemDto> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException($"User {id} was not found.");

        // Business rule: an admin can't lock themselves out by mistake.
        var isSelf = user.Id == _currentUser.UserId;
        if (isSelf && (request.Role != user.Role || request.IsActive == false))
        {
            throw new BadRequestException("You can't change your own role or deactivate your own account.");
        }

        user.Role = request.Role!.Value;
        user.IsActive = request.IsActive!.Value;
        await _db.SaveChangesAsync(cancellationToken);

        var orderCount = await _db.Orders.CountAsync(o => o.UserId == id, cancellationToken);
        return new UserListItemDto(
            user.Id, user.Email, user.FirstName, user.LastName, user.Role, user.IsActive,
            user.CreatedAt, user.LastLoginAt, orderCount);
    }

    public async Task<IReadOnlyList<UserOrderSummaryDto>> GetUserOrdersAsync(int id, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == id, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException($"User {id} was not found.");
        }

        return await _db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == id)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new UserOrderSummaryDto(
                o.Id, o.Status, o.TotalAmount, o.Items.Sum(i => i.Quantity), o.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
