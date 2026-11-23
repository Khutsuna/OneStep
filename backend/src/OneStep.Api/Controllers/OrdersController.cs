using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Checkout;
using OneStep.Application.Features.Orders;

namespace OneStep.Api.Controllers;

/// <summary>Checkout and orders. Customers see their own orders; staff see and manage all of them.</summary>
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>Lists orders, newest first. Customers get their own; staff get everyone's.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<OrderSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<PagedResult<OrderSummaryDto>> GetOrders(
        [FromQuery] OrderListRequest request, CancellationToken cancellationToken)
    {
        return await _orderService.GetOrdersAsync(request, cancellationToken);
    }

    /// <summary>One order with its items, totals and status timeline.</summary>
    /// <response code="404">No such order, or it belongs to someone else.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<OrderDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<OrderDetailsDto> GetById(int id, CancellationToken cancellationToken)
    {
        return await _orderService.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Checkout preview: subtotal, discounts, shipping, VAT and total for the current cart. Saves nothing.</summary>
    [HttpPost("quote")]
    [Authorize(Policy = Policies.CustomerOnly)]
    [ProducesResponseType<OrderTotals>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<OrderTotals> Quote(QuoteRequest request, CancellationToken cancellationToken)
    {
        return await _orderService.QuoteAsync(request, cancellationToken);
    }

    /// <summary>Places an order from the current cart.</summary>
    /// <response code="400">Empty cart, or an invalid address or delivery method.</response>
    /// <response code="409">Not enough stock for one of the products.</response>
    [HttpPost]
    [Authorize(Policy = Policies.CustomerOnly)]
    [ProducesResponseType<OrderDetailsDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDetailsDto>> Place(PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderService.PlaceOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>Changes an order's status (Paid, Shipped, Delivered, Cancelled). Staff only.</summary>
    /// <response code="409">That status change isn't allowed (e.g. Delivered -> Pending).</response>
    [HttpPut("{id:int}/status")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType<OrderDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<OrderDetailsDto> ChangeStatus(int id, ChangeOrderStatusRequest request, CancellationToken cancellationToken)
    {
        return await _orderService.ChangeStatusAsync(id, request, cancellationToken);
    }

    /// <summary>Cancels an order and returns its items to stock.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType<OrderDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<OrderDetailsDto> Cancel(int id, CancellationToken cancellationToken)
    {
        return await _orderService.CancelAsync(id, cancellationToken);
    }

    /// <summary>Test card payment: marks a Pending card order as Paid.</summary>
    [HttpPost("{id:int}/pay")]
    [Authorize(Policy = Policies.CustomerOnly)]
    [ProducesResponseType<OrderDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<OrderDetailsDto> Pay(int id, CancellationToken cancellationToken)
    {
        return await _orderService.PayAsync(id, cancellationToken);
    }
}
