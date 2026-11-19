using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Entities;

namespace OneStep.Application.Features.Products;

/// <summary>Product photos: upload, choose the main photo, delete.</summary>
public class ProductImageService : IProductImageService
{
    public const long MaxFileSize = 5 * 1024 * 1024;   // 5 MB
    private const int MaxImagesPerProduct = 8;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private readonly IAppDbContext _db;
    private readonly IFileStorage _storage;

    public ProductImageService(IAppDbContext db, IFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<ProductImageDto> UploadAsync(
        int productId, Stream content, string fileName, long length, CancellationToken cancellationToken)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == productId, cancellationToken))
        {
            throw new NotFoundException($"Product {productId} was not found.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new BadRequestException("Only .jpg, .jpeg, .png and .webp images are allowed.");
        }

        if (length is <= 0 or > MaxFileSize)
        {
            throw new BadRequestException("The image must be between 1 byte and 5 MB.");
        }

        var existing = await _db.ProductImages
            .Where(i => i.ProductId == productId)
            .Select(i => new { i.SortOrder })
            .ToListAsync(cancellationToken);

        if (existing.Count >= MaxImagesPerProduct)
        {
            throw new BadRequestException($"A product can have at most {MaxImagesPerProduct} images.");
        }

        // The file gets a new random name: the uploader's file name is never used on disk.
        var path = await _storage.SaveAsync(content, $"products/{productId}", extension, cancellationToken);

        var image = new ProductImage
        {
            ProductId = productId,
            FilePath = path,
            SortOrder = existing.Count == 0 ? 0 : existing.Max(i => i.SortOrder) + 1,
            IsMain = existing.Count == 0      // the first image becomes the main one
        };

        _db.ProductImages.Add(image);
        await _db.SaveChangesAsync(cancellationToken);

        return new ProductImageDto(image.Id, MediaUrl.From(image.FilePath)!, image.AltText, image.SortOrder, image.IsMain);
    }

    public async Task SetMainAsync(int productId, int imageId, CancellationToken cancellationToken)
    {
        var images = await _db.ProductImages.Where(i => i.ProductId == productId).ToListAsync(cancellationToken);
        var newMain = images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new NotFoundException($"Image {imageId} was not found for product {productId}.");

        // Two saves: first clear the old main image, then set the new one.
        // The database allows only one main image per product (filtered unique index),
        // so doing both in one save could briefly break that rule.
        foreach (var image in images.Where(i => i.IsMain && i.Id != imageId))
        {
            image.IsMain = false;
        }
        await _db.SaveChangesAsync(cancellationToken);

        newMain.IsMain = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int productId, int imageId, CancellationToken cancellationToken)
    {
        var image = await _db.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId, cancellationToken)
            ?? throw new NotFoundException($"Image {imageId} was not found for product {productId}.");

        _db.ProductImages.Remove(image);
        await _db.SaveChangesAsync(cancellationToken);

        // If the main image was deleted, the next one becomes main.
        if (image.IsMain)
        {
            var next = await _db.ProductImages
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.SortOrder)
                .FirstOrDefaultAsync(cancellationToken);
            if (next is not null)
            {
                next.IsMain = true;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        await _storage.DeleteAsync(image.FilePath, cancellationToken);
    }
}
