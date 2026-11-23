using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common;
using OneStep.Application.Common.Exceptions;
using OneStep.Application.Features.Pricing;
using OneStep.Domain.Enums;
using CartEntity = OneStep.Domain.Entities.Cart;
using CartItemEntity = OneStep.Domain.Entities.CartItem;

namespace OneStep.Application.Features.Cart;

/// <summary>
/// The logged-in customer's cart. The cart stores only products and quantities;
/// prices are always calculated fresh, so a campaign that starts or ends shows up immediately.
/// </summary>
public class CartService : ICartService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPricingService _pricing;

    public CartService(IAppDbContext db, ICurrentUser currentUser, IPricingService pricing)
    {
        _db = db;
        _currentUser = currentUser;
        _pricing = pricing;
    }

    private int UserId => _currentUser.UserId ?? throw new UnauthorizedException("Please log in.");

    public Task<CartDto> GetAsync(CancellationToken cancellationToken)
    {
        return BuildCartDtoAsync(cancellationToken);
    }

    public async Task<CartDto> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.Status == ProductStatus.Active, cancellationToken)
            ?? throw new NotFoundException("This product isn't available.");

        var cart = await _db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == UserId, cancellationToken);

        if (cart is null)
        {
            cart = new CartEntity { UserId = UserId };
            _db.Carts.Add(cart);
        }

        // Adding a product that is already in the cart raises its quantity instead of adding a second line.
        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        var newQuantity = (item?.Quantity ?? 0) + request.Quantity;

        if (newQuantity > product.StockQuantity)
        {
            throw new ConflictException(product.StockQuantity == 0
                ? $"{product.Name} is out of stock."
                : $"Only {product.StockQuantity} × {product.Name} in stock.");
        }

        if (item is null)
        {
            cart.Items.Add(new CartItemEntity { ProductId = request.ProductId, Quantity = request.Quantity });
        }
        else
        {
            item.Quantity = newQuantity;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await BuildCartDtoAsync(cancellationToken);
    }

    public async Task<CartDto> UpdateQuantityAsync(int itemId, UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var item = await FindOwnItemAsync(itemId, cancellationToken);

        if (request.Quantity > item.Product.StockQuantity)
        {
            throw new ConflictException($"Only {item.Product.StockQuantity} × {item.Product.Name} in stock.");
        }

        item.Quantity = request.Quantity;
        await _db.SaveChangesAsync(cancellationToken);
        return await BuildCartDtoAsync(cancellationToken);
    }

    public async Task<CartDto> RemoveItemAsync(int itemId, CancellationToken cancellationToken)
    {
        var item = await FindOwnItemAsync(itemId, cancellationToken);
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        return await BuildCartDtoAsync(cancellationToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        var items = await _db.CartItems.Where(i => i.Cart.UserId == UserId).ToListAsync(cancellationToken);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Only items in the CURRENT user's cart can be changed; anything else looks like it doesn't exist.</summary>
    private async Task<CartItemEntity> FindOwnItemAsync(int itemId, CancellationToken cancellationToken)
    {
        return await _db.CartItems
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == UserId, cancellationToken)
            ?? throw new NotFoundException("This item isn't in your cart.");
    }

    private async Task<CartDto> BuildCartDtoAsync(CancellationToken cancellationToken)
    {
        var rows = await _db.CartItems
            .AsNoTracking()
            .Where(i => i.Cart.UserId == UserId)
            .OrderBy(i => i.CreatedAt)
            .Select(i => new
            {
                i.Id,
                i.ProductId,
                i.Product.Name,
                i.Product.CategoryId,
                i.Product.Price,
                i.Product.StockQuantity,
                i.Quantity,
                ImagePath = i.Product.Images.Where(img => img.IsMain).Select(img => img.FilePath).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var prices = await _pricing.GetPricesAsync(
            rows.Select(r => new PricingInput(r.ProductId, r.CategoryId, r.Price)).ToList(), cancellationToken);

        var items = rows.Select(r =>
        {
            var price = prices[r.ProductId];
            return new CartItemDto(r.Id, r.ProductId, r.Name, MediaUrl.From(r.ImagePath),
                price.Price, price.FinalPrice, r.Quantity, r.StockQuantity, price.FinalPrice * r.Quantity);
        }).ToList();

        var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
        var total = items.Sum(i => i.LineTotal);
        return new CartDto(items, items.Sum(i => i.Quantity), subtotal, subtotal - total, total);
    }
}
