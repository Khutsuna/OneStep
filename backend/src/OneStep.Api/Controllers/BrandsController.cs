using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Brands;

namespace OneStep.Api.Controllers;

/// <summary>Product brands.</summary>
[ApiController]
[Route("api/brands")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    /// <summary>Lists all brands, A to Z, with their product counts.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<BrandDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<BrandDto>> GetAll(CancellationToken cancellationToken)
    {
        return await _brandService.GetAllAsync(cancellationToken);
    }

    /// <summary>Creates a brand. Staff only.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType<BrandDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BrandDto>> Create(BrandRequest request, CancellationToken cancellationToken)
    {
        var brand = await _brandService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), brand);
    }

    /// <summary>Renames a brand. Staff only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType<BrandDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<BrandDto> Update(int id, BrandRequest request, CancellationToken cancellationToken)
    {
        return await _brandService.UpdateAsync(id, request, cancellationToken);
    }

    /// <summary>Deletes a brand that has no products. Staff only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _brandService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
