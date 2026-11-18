using System.ComponentModel.DataAnnotations;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Users;

/// <summary>One row of the admin user list.</summary>
public record UserListItemDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    int OrderCount);

/// <summary>
/// What an admin can change about a user. Both fields are required (nullable + [Required]),
/// so a request that forgets one gets a 400 instead of silently resetting it.
/// </summary>
public class UpdateUserRequest
{
    [Required]
    public UserRole? Role { get; set; }

    [Required]
    public bool? IsActive { get; set; }
}

/// <summary>One order in a user's order history (admin view).</summary>
public record UserOrderSummaryDto(
    int Id,
    OrderStatus Status,
    decimal TotalAmount,
    int ItemCount,
    DateTime CreatedAt);
