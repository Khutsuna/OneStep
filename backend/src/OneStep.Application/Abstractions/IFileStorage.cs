namespace OneStep.Application.Abstractions;

/// <summary>
/// Saves and deletes uploaded files. The current implementation writes to the API's
/// wwwroot/uploads folder; moving to cloud storage later means writing one new class.
/// </summary>
public interface IFileStorage
{
    /// <summary>Saves the file under a new random name and returns its relative path, e.g. "products/12/3f9c...jpg".</summary>
    Task<string> SaveAsync(Stream content, string folder, string extension, CancellationToken cancellationToken);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken);
}
