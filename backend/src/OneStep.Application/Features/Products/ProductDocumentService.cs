using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Entities;

namespace OneStep.Application.Features.Products;

/// <summary>Product documents (PDF manuals, warranty cards): list for everyone, upload and delete for staff.</summary>
public class ProductDocumentService : IProductDocumentService
{
    private readonly IAppDbContext _db;
    private readonly IFileStorage _storage;

    public ProductDocumentService(IAppDbContext db, IFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<IReadOnlyList<ProductDocumentDto>> ListAsync(int productId, CancellationToken cancellationToken)
    {
        await EnsureProductExistsAsync(productId, cancellationToken);

        var documents = await _db.ProductDocuments
            .AsNoTracking()
            .Where(d => d.ProductId == productId)
            .OrderBy(d => d.CreatedAt)
            .ThenBy(d => d.Id)
            .ToListAsync(cancellationToken);

        return documents.Select(ToDto).ToList();
    }

    public async Task<ProductDocumentDto> UploadAsync(
        int productId, Stream content, string fileName, long length, string? title, CancellationToken cancellationToken)
    {
        await EnsureProductExistsAsync(productId, cancellationToken);

        if (!ProductDocumentRules.HasPdfExtension(fileName))
        {
            throw new BadRequestException("Only PDF files are allowed.");
        }

        if (length is <= 0 or > ProductDocumentRules.MaxFileSize)
        {
            throw new BadRequestException("The file must be between 1 byte and 20 MB.");
        }

        var count = await _db.ProductDocuments.CountAsync(d => d.ProductId == productId, cancellationToken);
        if (count >= ProductDocumentRules.MaxDocumentsPerProduct)
        {
            throw new BadRequestException(
                $"A product can have at most {ProductDocumentRules.MaxDocumentsPerProduct} documents.");
        }

        // Read the upload into memory (at most 20 MB) so its first bytes can be checked before saving.
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var start = buffer.GetBuffer().AsSpan(0, (int)Math.Min(buffer.Length, 8));
        if (!ProductDocumentRules.LooksLikePdf(start))
        {
            throw new BadRequestException("The file is not a valid PDF.");
        }

        buffer.Position = 0;
        var path = await _storage.SaveAsync(buffer, $"documents/{productId}", ".pdf", cancellationToken);

        var document = new ProductDocument
        {
            ProductId = productId,
            Title = ProductDocumentRules.MakeTitle(title, fileName),
            FileName = ProductDocumentRules.SafeFileName(fileName),
            FilePath = path,
            SizeBytes = buffer.Length
        };

        _db.ProductDocuments.Add(document);
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(document);
    }

    public async Task DeleteAsync(int productId, int documentId, CancellationToken cancellationToken)
    {
        var document = await _db.ProductDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.ProductId == productId, cancellationToken)
            ?? throw new NotFoundException($"Document {documentId} was not found for product {productId}.");

        _db.ProductDocuments.Remove(document);
        await _db.SaveChangesAsync(cancellationToken);

        // The row goes first, then the file: a failed file delete leaves an unused file, never a broken link.
        await _storage.DeleteAsync(document.FilePath, cancellationToken);
    }

    private async Task EnsureProductExistsAsync(int productId, CancellationToken cancellationToken)
    {
        // The soft-delete filter hides deleted products, so their documents are hidden too.
        if (!await _db.Products.AnyAsync(p => p.Id == productId, cancellationToken))
        {
            throw new NotFoundException($"Product {productId} was not found.");
        }
    }

    private static ProductDocumentDto ToDto(ProductDocument d) =>
        new(d.Id, d.Title, d.FileName, d.SizeBytes, MediaUrl.From(d.FilePath)!, d.CreatedAt);
}
