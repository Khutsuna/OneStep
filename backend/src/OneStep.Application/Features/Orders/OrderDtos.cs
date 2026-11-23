using System.ComponentModel.DataAnnotations;
using OneStep.Application.Features.Checkout;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Orders;

/// <summary>Checkout preview: totals for the current cart with a chosen delivery method.</summary>
public class QuoteRequest
{
    [Range(1, int.MaxValue)]
    public int DeliveryMethodId { get; set; }
}

public class PlaceOrderRequest
{
    /// <summary>An address from the customer's address book. It is copied onto the order.</summary>
    [Range(1, int.MaxValue)]
    public int AddressId { get; set; }

    [Range(1, int.MaxValue)]
    public int DeliveryMethodId { get; set; }

    [Required]
    public PaymentMethod? PaymentMethod { get; set; }

    [StringLength(500)]
    public string? CustomerNote { get; set; }
}

public class ChangeOrderStatusRequest
{
    [Required]
    public OrderStatus? Status { get; set; }

    [StringLength(300)]
    public string? Note { get; set; }
}

/// <summary>Filters for the order list. Customers always see only their own orders.</summary>
public class OrderListRequest
{
    public OrderStatus? Status { get; set; }

    /// <summary>Staff only: order number, customer name or email.</summary>
    public string? Search { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}

public record OrderSummaryDto(
    int Id,
    OrderStatus Status,
    decimal TotalAmount,
    int ItemCount,
    DateTime CreatedAt,
    string CustomerName,
    string CustomerEmail);

public record OrderItemDto(
    int ProductId,
    string ProductName,
    string Sku,
    string? ImageUrl,
    decimal UnitPrice,
    decimal UnitDiscount,
    int Quantity,
    decimal LineTotal);

public record ShippingAddressDto(
    string RecipientName,
    string Phone,
    string Line1,
    string? Line2,
    string City,
    string? Region,
    string? PostalCode,
    string CountryCode);

/// <summary>One step of the order-tracking timeline.</summary>
public record OrderStatusStepDto(
    OrderStatus? FromStatus,
    OrderStatus ToStatus,
    DateTime ChangedAt,
    string? Note,
    string? ChangedBy);

public record OrderDetailsDto(
    int Id,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime? PaidAt,
    PaymentMethod PaymentMethod,
    string DeliveryMethodName,
    string? CustomerNote,
    int CustomerId,
    string CustomerName,
    string CustomerEmail,
    ShippingAddressDto ShippingAddress,
    IReadOnlyList<OrderItemDto> Items,
    OrderTotals Totals,
    IReadOnlyList<OrderStatusStepDto> History,
    IReadOnlyList<OrderStatus> NextStatuses,   // for staff: which status buttons to show
    bool CanCancel,                            // for the current user
    bool CanPay);                              // for the customer: mock card payment
