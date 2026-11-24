using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Features.Account;

namespace OneStep.Api.Controllers;

/// <summary>The logged-in user's own profile and password.</summary>
[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService _account;

    public AccountController(IAccountService account)
    {
        _account = account;
    }

    /// <summary>Your profile.</summary>
    [HttpGet("profile")]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status200OK)]
    public async Task<ProfileDto> GetProfile(CancellationToken cancellationToken)
    {
        return await _account.GetProfileAsync(cancellationToken);
    }

    /// <summary>Changes your name and phone number.</summary>
    [HttpPut("profile")]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ProfileDto> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        return await _account.UpdateProfileAsync(request, cancellationToken);
    }

    /// <summary>Changes your password. Needs the current one.</summary>
    /// <response code="400">The current password is wrong, or the new one is too weak.</response>
    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await _account.ChangePasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
