using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Products;
using OneStep.Application.Features.Wishlist;

namespace OneStep.Api.Controllers;

/// <summary>The logged-in customer's wishlist.</summary>
[ApiController]
[Route("api/account/wishlist")]
[Authorize(Policy = Policies.CustomerOnly)]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlist;

    public WishlistController(IWishlistService wishlist)
    {
        _wishlist = wishlist;
    }

    /// <summary>The saved products, newest first, with current prices.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductListItemDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<ProductListItemDto>> Get(CancellationToken cancellationToken)
    {
        return await _wishlist.GetAsync(cancellationToken);
    }

    /// <summary>Only the product ids: enough to colour the heart buttons.</summary>
    [HttpGet("ids")]
    [ProducesResponseType<IReadOnlyList<int>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<int>> GetIds(CancellationToken cancellationToken)
    {
        return await _wishlist.GetProductIdsAsync(cancellationToken);
    }

    /// <summary>Saves a product. Saving it again does nothing (idempotent PUT).</summary>
    /// <response code="404">No active product has this id.</response>
    [HttpPut("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Add(int productId, CancellationToken cancellationToken)
    {
        await _wishlist.AddAsync(productId, cancellationToken);
        return NoContent();
    }

    /// <summary>Removes a product. Removing one that isn't saved does nothing.</summary>
    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken)
    {
        await _wishlist.RemoveAsync(productId, cancellationToken);
        return NoContent();
    }
}
