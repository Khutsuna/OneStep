using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Cart;

namespace OneStep.Api.Controllers;

/// <summary>The logged-in customer's shopping cart.</summary>
[ApiController]
[Route("api/cart")]
[Authorize(Policy = Policies.CustomerOnly)]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>Returns the cart with current prices.</summary>
    [HttpGet]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    public async Task<CartDto> Get(CancellationToken cancellationToken)
    {
        return await _cartService.GetAsync(cancellationToken);
    }

    /// <summary>Adds a product (or raises its quantity if it's already in the cart).</summary>
    /// <response code="409">Not enough stock.</response>
    [HttpPost("items")]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<CartDto> AddItem(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        return await _cartService.AddItemAsync(request, cancellationToken);
    }

    /// <summary>Changes the quantity of one cart line.</summary>
    [HttpPut("items/{id:int}/quantity")]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<CartDto> UpdateQuantity(int id, UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        return await _cartService.UpdateQuantityAsync(id, request, cancellationToken);
    }

    /// <summary>Removes one cart line.</summary>
    [HttpDelete("items/{id:int}")]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<CartDto> RemoveItem(int id, CancellationToken cancellationToken)
    {
        return await _cartService.RemoveItemAsync(id, cancellationToken);
    }

    /// <summary>Empties the cart.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        await _cartService.ClearAsync(cancellationToken);
        return NoContent();
    }
}
