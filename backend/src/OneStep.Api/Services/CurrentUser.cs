using OneStep.Application.Abstractions;

namespace OneStep.Api.Services;

/// <summary>Reads the logged-in user from the claims of the validated JWT.</summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public int? UserId =>
        int.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value, out var id) ? id : null;

    public string? Role =>
        _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;
}
