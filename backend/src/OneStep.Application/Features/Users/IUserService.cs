using OneStep.Application.Common;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Users;

public interface IUserService
{
    Task<PagedResult<UserListItemDto>> GetUsersAsync(
        string? search, UserRole? role, int page, int pageSize, CancellationToken cancellationToken);

    Task<UserListItemDto> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserOrderSummaryDto>> GetUserOrdersAsync(int id, CancellationToken cancellationToken);
}
