using Microsoft.AspNetCore.Hosting;
using OneStep.Application.Abstractions;

namespace OneStep.Infrastructure.Storage;

/// <summary>
/// Stores uploads in the API's wwwroot/uploads folder, which the API serves as static files
/// (e.g. https://localhost/uploads/products/12/3f9c...jpg).
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(IWebHostEnvironment environment)
    {
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        _rootPath = Path.GetFullPath(Path.Combine(webRoot, "uploads"));
    }

    public async Task<string> SaveAsync(Stream content, string folder, string extension, CancellationToken cancellationToken)
    {
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = $"{folder}/{fileName}";
        var fullPath = GetSafeFullPath(relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using var file = File.Create(fullPath);
        await content.CopyToAsync(file, cancellationToken);

        return relativePath;
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = GetSafeFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    /// <summary>Makes sure a path can never point outside the uploads folder (e.g. "../../appsettings.json").</summary>
    private string GetSafeFullPath(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));
        if (!fullPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid file path.");
        }
        return fullPath;
    }
}
