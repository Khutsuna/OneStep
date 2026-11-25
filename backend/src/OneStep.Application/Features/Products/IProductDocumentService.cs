namespace OneStep.Application.Features.Products;

public interface IProductDocumentService
{
    Task<IReadOnlyList<ProductDocumentDto>> ListAsync(int productId, CancellationToken cancellationToken);

    Task<ProductDocumentDto> UploadAsync(
        int productId, Stream content, string fileName, long length, string? title, CancellationToken cancellationToken);

    Task DeleteAsync(int productId, int documentId, CancellationToken cancellationToken);
}
