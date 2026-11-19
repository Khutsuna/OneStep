namespace OneStep.Application.Features.Products;

public interface IProductImageService
{
    Task<ProductImageDto> UploadAsync(
        int productId, Stream content, string fileName, long length, CancellationToken cancellationToken);

    Task SetMainAsync(int productId, int imageId, CancellationToken cancellationToken);
    Task DeleteAsync(int productId, int imageId, CancellationToken cancellationToken);
}
