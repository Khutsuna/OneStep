using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Users;
using OneStep.Domain.Enums;

namespace OneStep.Api.Controllers;

/// <summary>User management. Administrators only.</summary>
[ApiController]
[Route("api/users")]
[Authorize(Policy = Policies.AdminOnly)]   // applies to every endpoint in this controller
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Lists users, newest first, with optional search and role filter.</summary>
    /// <param name="search">Part of an email, first name or last name.</param>
    /// <param name="role">Only users with this role.</param>
    /// <param name="page">Page number, starting at 1.</param>
    /// <param name="pageSize">Users per page (1 to 100).</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="401">No valid token.</response>
    /// <response code="403">Logged in, but not an Admin.</response>
    [HttpGet]
    [ProducesResponseType<PagedResult<UserListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<PagedResult<UserListItemDto>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] UserRole? role,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _userService.GetUsersAsync(search, role, page, pageSize, cancellationToken);
    }

    /// <summary>Changes a user's role or active status.</summary>
    /// <response code="400">Invalid input, or an admin trying to demote or deactivate themselves.</response>
    /// <response code="404">No user has this id.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<UserListItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<UserListItemDto> UpdateUser(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        return await _userService.UpdateUserAsync(id, request, cancellationToken);
    }

    /// <summary>Lists one user's orders, newest first.</summary>
    /// <response code="404">No user has this id.</response>
    [HttpGet("{id:int}/orders")]
    [ProducesResponseType<IReadOnlyList<UserOrderSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IReadOnlyList<UserOrderSummaryDto>> GetUserOrders(int id, CancellationToken cancellationToken)
    {
        return await _userService.GetUserOrdersAsync(id, cancellationToken);
    }
}
