using System.ComponentModel.DataAnnotations;

namespace OneStep.Application.Features.Account;

public record ProfileDto(int Id, string Email, string FirstName, string LastName, string? PhoneNumber, DateTime CreatedAt);

public class UpdateProfileRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(30)]
    [RegularExpression(@"^[+0-9 ()-]{6,}$", ErrorMessage = "Use digits, spaces and +, e.g. +995 555 12 34 56.")]
    public string? PhoneNumber { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    // Same rules as registration.
    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one letter and one digit.")]
    public string NewPassword { get; set; } = string.Empty;
}
