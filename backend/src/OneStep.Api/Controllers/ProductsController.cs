using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Products;

namespace OneStep.Api.Controllers;

/// <summary>Products: browsing for everyone, management for staff.</summary>
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IProductImageService _imageService;
    private readonly IProductDocumentService _documentService;

    public ProductsController(
        IProductService productService, IProductImageService imageService, IProductDocumentService documentService)
    {
        _productService = productService;
        _imageService = imageService;
        _documentService = documentService;
    }

    /// <summary>Searches products with filters, sorting and paging.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PagedResult<ProductListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<PagedResult<ProductListItemDto>> Search(
        [FromQuery] ProductSearchRequest request, CancellationToken cancellationToken)
    {
        return await _productService.SearchAsync(request, cancellationToken);
    }

    /// <summary>Gets the full details of one product: images, attributes, rating and current price.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType<ProductDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ProductDetailsDto> GetById(int id, CancellationToken cancellationToken)
    {
        return await _productService.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Options for the filter panel (brands, price range, attribute values), optionally within one category.</summary>
    [HttpGet("filters")]
    [AllowAnonymous]
    [ProducesResponseType<ProductFiltersDto>(StatusCodes.Status200OK)]
    public async Task<ProductFiltersDto> GetFilters([FromQuery] int? categoryId, CancellationToken cancellationToken)
    {
        return await _productService.GetFiltersAsync(categoryId, cancellationToken);
    }

    /// <summary>Products from the same category with the closest price.</summary>
    [HttpGet("{id:int}/similar")]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<ProductListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IReadOnlyList<ProductListItemDto>> GetSimilar(
        int id, CancellationToken cancellationToken, [FromQuery] int count = 4)
    {
        return await _productService.GetSimilarAsync(id, count, cancellationToken);
    }

    /// <summary>The product's base price over time (filled by the SQL trigger trg_Products_PriceHistory).</summary>
    [HttpGet("{id:int}/price-history")]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<PricePointDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IReadOnlyList<PricePointDto>> GetPriceHistory(int id, CancellationToken cancellationToken)
    {
        return await _productService.GetPriceHistoryAsync(id, cancellationToken);
    }

    /// <summary>Products at or below their minimum stock level. Staff only.</summary>
    [HttpGet("low-stock")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType<IReadOnlyList<LowStockProductDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<LowStockProductDto>> GetLowStock(CancellationToken cancellationToken)
    {
        return await _productService.GetLowStockAsync(cancellationToken);
    }

    /// <summary>Creates a product. Staff only.</summary>
    /// <response code="400">Invalid input, or a category, brand or attribute that doesn't exist.</response>
    /// <response code="409">Another product already uses this SKU.</response>
    [HttpPost]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType<ProductDetailsDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDetailsDto>> Create(ProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Updates a product (name, price, stock, status, attributes...). Staff only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType<ProductDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ProductDetailsDto> Update(int id, ProductRequest request, CancellationToken cancellationToken)
    {
        return await _productService.UpdateAsync(id, request, cancellationToken);
    }

    /// <summary>Sets the stock quantity. Creates a low-stock alert when it drops to the threshold. Staff only.</summary>
    [HttpPatch("{id:int}/stock")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStock(int id, StockUpdateRequest request, CancellationToken cancellationToken)
    {
        await _productService.UpdateStockAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a product (soft delete: past orders keep showing it). Staff only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Uploads one product image (.jpg, .png or .webp, max 5 MB). Staff only.</summary>
    [HttpPost("{id:int}/images")]
    [Authorize(Policy = Policies.StaffOnly)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ProductImageService.MaxFileSize + 100_000)]
    [ProducesResponseType<ProductImageDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductImageDto>> UploadImage(int id, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var image = await _imageService.UploadAsync(id, stream, file.FileName, file.Length, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, image);
    }

    /// <summary>Makes an image the product's main image. Staff only.</summary>
    [HttpPut("{id:int}/images/{imageId:int}/main")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetMainImage(int id, int imageId, CancellationToken cancellationToken)
    {
        await _imageService.SetMainAsync(id, imageId, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a product image. Staff only.</summary>
    [HttpDelete("{id:int}/images/{imageId:int}")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int id, int imageId, CancellationToken cancellationToken)
    {
        await _imageService.DeleteAsync(id, imageId, cancellationToken);
        return NoContent();
    }

    /// <summary>Lists the product's downloadable documents (PDF manuals, warranty cards).</summary>
    [HttpGet("{id:int}/documents")]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<ProductDocumentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IReadOnlyList<ProductDocumentDto>> GetDocuments(int id, CancellationToken cancellationToken)
    {
        return await _documentService.ListAsync(id, cancellationToken);
    }

    /// <summary>Uploads a PDF document for the product (max 20 MB, 10 per product). Staff only.</summary>
    [HttpPost("{id:int}/documents")]
    [Authorize(Policy = Policies.StaffOnly)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ProductDocumentRules.MaxFileSize + 100_000)]
    [ProducesResponseType<ProductDocumentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDocumentDto>> UploadDocument(
        int id, IFormFile file, [FromForm] string? title, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var document = await _documentService.UploadAsync(id, stream, file.FileName, file.Length, title, cancellationToken);
        return CreatedAtAction(nameof(GetDocuments), new { id }, document);
    }

    /// <summary>Deletes a product document. Staff only.</summary>
    [HttpDelete("{id:int}/documents/{documentId:int}")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(int id, int documentId, CancellationToken cancellationToken)
    {
        await _documentService.DeleteAsync(id, documentId, cancellationToken);
        return NoContent();
    }
}
