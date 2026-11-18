using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OneStep.Application.Abstractions;
using OneStep.Application.Features.Auth;

namespace OneStep.Api.Controllers;

/// <summary>Registration, login and the current user's details.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;

    public AuthController(IAuthService authService, ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>Creates a new customer account and logs it in.</summary>
    /// <response code="201">Account created. The response contains a JWT.</response>
    /// <response code="400">Invalid input, e.g. a malformed email or a weak password.</response>
    /// <response code="409">This email is already registered.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Me), response);
    }

    /// <summary>Logs in with email and password and returns a JWT.</summary>
    /// <response code="401">Wrong email or password.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request, cancellationToken);
    }

    /// <summary>Returns the logged-in user. Requires a token.</summary>
    /// <response code="401">No token, or the token is invalid or expired.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<UserDto> Me(CancellationToken cancellationToken)
    {
        // The user id comes from the validated token, never from the request body.
        return await _authService.GetUserAsync(_currentUser.UserId!.Value, cancellationToken);
    }
}