using System.ComponentModel.DataAnnotations;

namespace OneStep.Application.Features.Auth;

// What the client sends to register. The [attributes] are validation rules:
// if any rule fails, ASP.NET Core returns 400 automatically, before your code even runs.
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one letter and one digit.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

// What the API sends back. Notice: no PasswordHash. DTOs decide exactly what leaves the API.
public record UserDto(int Id, string Email, string FirstName, string LastName, string Role);

public record AuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);