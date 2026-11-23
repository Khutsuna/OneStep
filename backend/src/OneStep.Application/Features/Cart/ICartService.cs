namespace OneStep.Application.Features.Cart;

public interface ICartService
{
    Task<CartDto> GetAsync(CancellationToken cancellationToken);
    Task<CartDto> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken);
    Task<CartDto> UpdateQuantityAsync(int itemId, UpdateCartItemRequest request, CancellationToken cancellationToken);
    Task<CartDto> RemoveItemAsync(int itemId, CancellationToken cancellationToken);
    Task ClearAsync(CancellationToken cancellationToken);
}
