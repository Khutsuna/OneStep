using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Categories;

namespace OneStep.Api.Controllers;

/// <summary>Product categories and subcategories.</summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>Returns all categories as a tree (subcategories nested inside their parent).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<CategoryTreeDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<CategoryTreeDto>> GetTree(CancellationToken cancellationToken)
    {
        return await _categoryService.GetTreeAsync(cancellationToken);
    }

    /// <summary>Creates a category. Staff only.</summary>
    /// <response code="409">A category with this name already exists at this level.</response>
    [HttpPost]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType<CategoryTreeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryTreeDto>> Create(CategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTree), category);
    }

    /// <summary>Renames or moves a category. Staff only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType<CategoryTreeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<CategoryTreeDto> Update(int id, CategoryRequest request, CancellationToken cancellationToken)
    {
        return await _categoryService.UpdateAsync(id, request, cancellationToken);
    }

    /// <summary>Deletes an empty category. Staff only.</summary>
    /// <response code="409">The category still has subcategories or products.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
