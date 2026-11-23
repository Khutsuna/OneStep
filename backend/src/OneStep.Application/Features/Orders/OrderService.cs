using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common;
using OneStep.Application.Common.Exceptions;
using OneStep.Application.Features.Checkout;
using OneStep.Application.Features.Inventory;
using OneStep.Application.Features.Pricing;
using OneStep.Domain.Entities;
using OneStep.Domain.Enums;
using OneStep.Domain.Rules;
using OneStep.Domain.ValueObjects;

namespace OneStep.Application.Features.Orders;

public class OrderService : IOrderService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPricingService _pricing;

    public OrderService(IAppDbContext db, ICurrentUser currentUser, IPricingService pricing)
    {
        _db = db;
        _currentUser = currentUser;
        _pricing = pricing;
    }

    private int UserId => _currentUser.UserId ?? throw new UnauthorizedException("Please log in.");
    private bool IsStaff => _currentUser.Role is "Admin" or "StoreManager";

    // =====================================================================
    //  [You] Placing an order. Replace this method with your own code.
    // =====================================================================
    public async Task<OrderDetailsDto> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        // 1. Load what the customer is buying, where it goes and how.
        var cart = await LoadCartAsync(cancellationToken);
        if (cart.Count == 0)
        {
            throw new BadRequestException("Your cart is empty.");
        }

        var address = await _db.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == UserId, cancellationToken)
            ?? throw new BadRequestException("Please choose an address from your address book.");

        var delivery = await LoadDeliveryMethodAsync(request.DeliveryMethodId, cancellationToken);

        // 2. Check that every product is still in stock.
        foreach (var item in cart)
        {
            if (item.Quantity > item.Product.StockQuantity)
            {
                throw new ConflictException(
                    $"Only {item.Product.StockQuantity} × {item.Product.Name} left in stock. Please update your cart.");
            }
        }

        // 3. Current prices (with campaign discounts) and the totals.
        var pricedLines = await PriceCartAsync(cart, cancellationToken);
        var totals = CheckoutCalculator.Calculate(
            pricedLines.Select(p => p.Line).ToList(), delivery.Price, delivery.FreeOverAmount);

        // 4. Build the order. Names, prices and the address are COPIED,
        //    so the order never changes when products or addresses change later.
        var order = new Order
        {
            UserId = UserId,
            Status = OrderStatus.Pending,
            Subtotal = totals.Subtotal,
            DiscountTotal = totals.DiscountTotal,
            ShippingCost = totals.ShippingCost,
            TaxAmount = totals.TaxAmount,
            TotalAmount = totals.TotalAmount,
            DeliveryMethodId = delivery.Id,
            PaymentMethod = request.PaymentMethod!.Value,
            CustomerNote = string.IsNullOrWhiteSpace(request.CustomerNote) ? null : request.CustomerNote.Trim(),
            ShippingAddress = new ShippingAddress
            {
                RecipientName = address.RecipientName,
                Phone = address.Phone,
                Line1 = address.Line1,
                Line2 = address.Line2,
                City = address.City,
                Region = address.Region,
                PostalCode = address.PostalCode,
                CountryCode = address.CountryCode
            }
        };

        foreach (var (item, line) in pricedLines)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Sku = item.Product.Sku,
                UnitPrice = line.UnitPrice,
                UnitDiscount = line.UnitDiscount,
                Quantity = item.Quantity
            });

            // 5. Take the items out of stock, and alert staff if stock gets low.
            var previousQuantity = item.Product.StockQuantity;
            item.Product.StockQuantity -= item.Quantity;
            var alert = StockAlerts.CreateIfNeeded(item.Product, previousQuantity);
            if (alert is not null)
            {
                _db.Notifications.Add(alert);
            }
        }

        // 6. The first step of the tracking timeline, and a "new order" alert for staff.
        order.StatusHistory.Add(new OrderStatusHistory
        {
            ToStatus = OrderStatus.Pending,
            ChangedByUserId = UserId,
            Note = "Order placed"
        });
        _db.Orders.Add(order);
        _db.Notifications.Add(new Notification
        {
            Type = NotificationType.NewOrder,
            Message = $"New order from {address.RecipientName}: {totals.TotalAmount:0.00} GEL",
            Order = order
        });

        // 7. Empty the cart.
        _db.CartItems.RemoveRange(cart);

        // 8. Save EVERYTHING at once. EF runs one SaveChanges as one database transaction:
        //    the order, the stock changes and the empty cart are either all saved, or none of them.
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone else bought the same product a moment earlier (its RowVersion changed).
            throw new ConflictException("Stock changed while you were checking out. Please check your cart and try again.");
        }

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    // =====================================================================
    //  Everything below is ready to use.
    // =====================================================================

    public async Task<OrderTotals> QuoteAsync(QuoteRequest request, CancellationToken cancellationToken)
    {
        var cart = await LoadCartAsync(cancellationToken);
        if (cart.Count == 0)
        {
            throw new BadRequestException("Your cart is empty.");
        }

        var delivery = await LoadDeliveryMethodAsync(request.DeliveryMethodId, cancellationToken);
        var lines = await PriceCartAsync(cart, cancellationToken);

        return CheckoutCalculator.Calculate(
            lines.Select(l => l.Line).ToList(), delivery.Price, delivery.FreeOverAmount);
    }

    public async Task<PagedResult<OrderSummaryDto>> GetOrdersAsync(OrderListRequest request, CancellationToken cancellationToken)
    {
        var query = _db.Orders.AsNoTracking();

        // Customers only ever see their own orders.
        if (!IsStaff)
        {
            query = query.Where(o => o.UserId == UserId);
        }

        if (request.Status is not null)
        {
            query = query.Where(o => o.Status == request.Status);
        }

        if (IsStaff && !string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            var isNumber = int.TryParse(term, out var orderId);
            query = query.Where(o =>
                (isNumber && o.Id == orderId) ||
                o.User.Email.Contains(term) ||
                (o.User.FirstName + " " + o.User.LastName).Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto(
                o.Id, o.Status, o.TotalAmount, o.Items.Sum(i => i.Quantity), o.CreatedAt,
                o.User.FirstName + " " + o.User.LastName, o.User.Email))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderSummaryDto>(items, request.Page, request.PageSize, totalCount);
    }

    public async Task<OrderDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.DeliveryMethod)
            .Include(o => o.Items)
            .Include(o => o.StatusHistory).ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        // Someone else's order returns 404, not 403, so order numbers can't be probed.
        if (order is null || (!IsStaff && order.UserId != UserId))
        {
            throw new NotFoundException($"Order {id} was not found.");
        }

        var productIds = order.Items.Select(i => i.ProductId).ToList();
        var images = await _db.ProductImages.AsNoTracking()
            .Where(i => productIds.Contains(i.ProductId) && i.IsMain)
            .ToDictionaryAsync(i => i.ProductId, i => i.FilePath, cancellationToken);

        return ToDetails(order, images);
    }

    public async Task<OrderDetailsDto> ChangeStatusAsync(int id, ChangeOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var order = await LoadOrderForUpdateAsync(id, cancellationToken);
        await MoveToStatusAsync(order, request.Status!.Value, request.Note, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<OrderDetailsDto> CancelAsync(int id, CancellationToken cancellationToken)
    {
        var order = await LoadOrderForUpdateAsync(id, cancellationToken);

        // Customers can cancel only while the order is Pending; staff also while it is Paid.
        if (!IsStaff && (order.UserId != UserId || order.Status != OrderStatus.Pending))
        {
            throw new BadRequestException("This order can no longer be cancelled.");
        }

        await MoveToStatusAsync(order, OrderStatus.Cancelled,
            IsStaff ? "Cancelled by staff" : "Cancelled by customer", cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Mock card payment: there is no real payment provider, it simply marks the order as paid.</summary>
    public async Task<OrderDetailsDto> PayAsync(int id, CancellationToken cancellationToken)
    {
        var order = await LoadOrderForUpdateAsync(id, cancellationToken);

        if (order.UserId != UserId)
        {
            throw new NotFoundException($"Order {id} was not found.");
        }
        if (order.PaymentMethod != PaymentMethod.Card)
        {
            throw new BadRequestException("This order is paid in cash on delivery.");
        }

        await MoveToStatusAsync(order, OrderStatus.Paid, "Card payment received (test payment)", cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    // ---------- helpers your PlaceOrderAsync can use ----------

    /// <summary>The current user's cart lines, with their products (tracked, so stock can be changed).</summary>
    private async Task<List<CartItem>> LoadCartAsync(CancellationToken cancellationToken)
    {
        return await _db.CartItems
            .Include(i => i.Product)
            .Where(i => i.Cart.UserId == UserId)
            .ToListAsync(cancellationToken);
    }

    private async Task<DeliveryMethod> LoadDeliveryMethodAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.DeliveryMethods.FirstOrDefaultAsync(d => d.Id == id && d.IsActive, cancellationToken)
            ?? throw new BadRequestException("Please choose an available delivery method.");
    }

    /// <summary>Current prices (after campaigns) for each cart line.</summary>
    private async Task<List<(CartItem Item, CheckoutLine Line)>> PriceCartAsync(
        List<CartItem> cart, CancellationToken cancellationToken)
    {
        var prices = await _pricing.GetPricesAsync(
            cart.Select(i => new PricingInput(i.ProductId, i.Product.CategoryId, i.Product.Price)).ToList(),
            cancellationToken);

        return cart
            .Select(i => (i, new CheckoutLine(prices[i.ProductId].Price, prices[i.ProductId].Discount, i.Quantity)))
            .ToList();
    }

    // ---------- private helpers ----------

    private async Task<Order> LoadOrderForUpdateAsync(int id, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null || (!IsStaff && order.UserId != UserId))
        {
            throw new NotFoundException($"Order {id} was not found.");
        }
        return order;
    }

    /// <summary>The one place where an order's status changes. Uses the state machine and records the history.</summary>
    private async Task MoveToStatusAsync(Order order, OrderStatus next, string? note, CancellationToken cancellationToken)
    {
        if (!OrderStatusTransitions.CanMove(order.Status, next))
        {
            throw new ConflictException($"An order can't go from {order.Status} to {next}.");
        }

        order.StatusHistory.Add(new OrderStatusHistory
        {
            FromStatus = order.Status,
            ToStatus = next,
            ChangedByUserId = UserId,
            Note = note
        });
        order.Status = next;

        if (next == OrderStatus.Paid)
        {
            order.PaidAt = DateTime.UtcNow;
        }

        // A cancelled order gives its items back to stock.
        if (next == OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                item.Product.StockQuantity += item.Quantity;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private OrderDetailsDto ToDetails(Order order, IReadOnlyDictionary<int, string> images)
    {
        var a = order.ShippingAddress;
        var isOwner = order.UserId == _currentUser.UserId;

        var canCancel = OrderStatusTransitions.CanMove(order.Status, OrderStatus.Cancelled)
                        && (IsStaff || (isOwner && order.Status == OrderStatus.Pending));
        var canPay = isOwner && order.Status == OrderStatus.Pending && order.PaymentMethod == PaymentMethod.Card;

        return new OrderDetailsDto(
            order.Id, order.Status, order.CreatedAt, order.PaidAt, order.PaymentMethod,
            order.DeliveryMethod.Name, order.CustomerNote,
            order.UserId, $"{order.User.FirstName} {order.User.LastName}", order.User.Email,
            new ShippingAddressDto(a.RecipientName, a.Phone, a.Line1, a.Line2, a.City, a.Region, a.PostalCode, a.CountryCode),
            order.Items.OrderBy(i => i.Id).Select(i => new OrderItemDto(
                i.ProductId, i.ProductName, i.Sku, MediaUrl.From(images.GetValueOrDefault(i.ProductId)),
                i.UnitPrice, i.UnitDiscount, i.Quantity, i.LineTotal)).ToList(),
            new OrderTotals(order.Subtotal, order.DiscountTotal, order.ShippingCost, order.TaxAmount, order.TotalAmount),
            order.StatusHistory.OrderBy(h => h.CreatedAt).ThenBy(h => h.Id).Select(h => new OrderStatusStepDto(
                h.FromStatus, h.ToStatus, h.CreatedAt, h.Note,
                h.ChangedByUser is null ? null : $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}")).ToList(),
            IsStaff ? OrderStatusTransitions.NextStatuses(order.Status) : [],
            canCancel,
            canPay);
    }
}
